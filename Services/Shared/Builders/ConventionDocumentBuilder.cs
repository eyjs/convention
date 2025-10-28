// ----------------- ConventionDocumentBuilder.cs (수정된 최종 버전) -----------------

using LocalRAG.Data;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using LocalRAG.DTOs.AiModels;

namespace LocalRAG.Services.Shared.Builders
{
    public class ConventionDocumentBuilder
    {

        public Task<List<DocumentChunk>> BuildDocumentChunks(ConventionIndexingData data)
        {
            var chunks = new List<DocumentChunk>();

            // DTO에서 필요한 데이터를 꺼내서 사용합니다.
            var convention = data.Convention;
            var notices = data.Notices;

            // 1. 행사 기본 정보 청크 (기존과 유사하지만 더 많은 정보 추가)
            var conventionInfoSb = new StringBuilder();
            conventionInfoSb.AppendLine($"# 행사명: {convention.Title}");
            conventionInfoSb.AppendLine($"- 기간: {convention.StartDate:yyyy년 MM월 dd일} ~ {convention.EndDate:yyyy년 MM월 dd일}");
            conventionInfoSb.AppendLine($"- 종류: {convention.ConventionType}");
            // 여기에 행사 개요(Description) 같은 공용 정보가 있다면 추가하는 것이 좋습니다.
            // conventionInfoSb.AppendLine($"- 개요: {convention.Description}");

            chunks.Add(new DocumentChunk
            {
                Content = conventionInfoSb.ToString(),
                Metadata = new Dictionary<string, object>
                {
                    { "type", "convention_info" },
                    { "conventionId", convention.Id },
                    { "sourceType", "Convention" }
                }
            });

            // 2. 행사 전체 참석자 정보 청크 (개인정보 제외)
            if (convention.Guests != null && convention.Guests.Any())
            {
                var guestSummarySb = new StringBuilder();
                guestSummarySb.AppendLine($"# {convention.Title} 행사 참석자 요약");
                guestSummarySb.AppendLine($"- 총 참석자 수: {convention.Guests.Count}명");

                var corpPartCounts = convention.Guests
                    .Where(g => !string.IsNullOrEmpty(g.CorpPart))
                    .GroupBy(g => g.CorpPart)
                    .Select(g => new { CorpPart = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                if (corpPartCounts.Any())
                {
                    guestSummarySb.AppendLine("## 부서별 참석자 수:");
                    foreach (var cp in corpPartCounts)
                    {
                        guestSummarySb.AppendLine($"- {cp.CorpPart}: {cp.Count}명");
                    }
                }

                var affiliationCounts = convention.Guests
                    .Where(g => !string.IsNullOrEmpty(g.Affiliation))
                    .GroupBy(g => g.Affiliation)
                    .Select(g => new { Affiliation = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                if (affiliationCounts.Any())
                {
                    guestSummarySb.AppendLine("## 소속별 참석자 수:");
                    foreach (var aff in affiliationCounts)
                    {
                        guestSummarySb.AppendLine($"- {aff.Affiliation}: {aff.Count}명");
                    }
                }

                chunks.Add(new DocumentChunk
                {
                    Content = guestSummarySb.ToString(),
                    Metadata = new Dictionary<string, object>
                    {
                        { "type", "guest_summary" },
                        { "conventionId", convention.Id },
                        { "sourceType", "GuestSummary" }
                    }
                });
            }

            // 3. 공지사항 정보 청크
            if (notices.Any())
            {
                // 3.1. 고정 공지 청크
                var pinnedNotices = notices.Where(n => n.IsPinned).ToList();
                if (pinnedNotices.Any())
                {
                    var pinnedSb = new StringBuilder();
                    pinnedSb.AppendLine($"# {convention.Title} 행사 고정 공지사항");
                    foreach (var notice in pinnedNotices)
                    {
                        pinnedSb.AppendLine($"## 제목: {notice.Title}");
                        pinnedSb.AppendLine($"- 내용: {notice.Content}");
                        pinnedSb.AppendLine($"- 게시일: {notice.CreatedAt:yyyy년 MM월 dd일}");
                    }
                    chunks.Add(new DocumentChunk
                    {
                        Content = pinnedSb.ToString(),
                        Metadata = new Dictionary<string, object>
                        {
                            { "type", "pinned_notices" },
                            { "conventionId", convention.Id },
                            { "sourceType", "Notice" }
                        }
                    });
                }

                // 3.2. 일반 공지사항 요약 청크
                var noticeSummarySb = new StringBuilder();
                noticeSummarySb.AppendLine($"# {convention.Title} 행사 공지사항 요약");
                noticeSummarySb.AppendLine($"- 총 공지사항 수: {notices.Count}개");

                var nonPinnedNotices = notices.Where(n => !n.IsPinned).Take(5).ToList(); // 최근 5개만 요약
                if (nonPinnedNotices.Any())
                {
                    noticeSummarySb.AppendLine("## 최근 공지사항 (최대 5개):");
                    foreach (var notice in nonPinnedNotices)
                    {
                        noticeSummarySb.AppendLine($"- 제목: {notice.Title} (게시일: {notice.CreatedAt:yyyy년 MM월 dd일})");
                    }
                }
                chunks.Add(new DocumentChunk
                {
                    Content = noticeSummarySb.ToString(),
                    Metadata = new Dictionary<string, object>
                    {
                        { "type", "notice_summary" },
                        { "conventionId", convention.Id },
                        { "sourceType", "Notice" }
                    }
                });
            }

            // 4. 일정 정보 청크
            if (convention.ScheduleTemplates != null && convention.ScheduleTemplates.Any())
            {
                foreach (var template in convention.ScheduleTemplates)
                {
                    if (template.ScheduleItems != null && template.ScheduleItems.Any())
                    {
                        var scheduleSb = new StringBuilder();
                        scheduleSb.AppendLine($"# {template.CourseName} 일정표");

                        var groupedByDate = template.ScheduleItems
                            .OrderBy(i => i.ScheduleDate)
                            .ThenBy(i => i.OrderNum)
                            .GroupBy(i => i.ScheduleDate.Date);

                        foreach (var dateGroup in groupedByDate)
                        {
                            scheduleSb.AppendLine($"## {dateGroup.Key:yyyy년 MM월 dd일 (ddd)}");

                            foreach (var item in dateGroup)
                            {
                                scheduleSb.AppendLine($"### {item.Title}");

                                if (!string.IsNullOrEmpty(item.StartTime) || !string.IsNullOrEmpty(item.EndTime))
                                {
                                    var timeStr = !string.IsNullOrEmpty(item.StartTime) && !string.IsNullOrEmpty(item.EndTime)
                                        ? $"{item.StartTime} - {item.EndTime}"
                                        : !string.IsNullOrEmpty(item.StartTime)
                                            ? item.StartTime
                                            : $"~ {item.EndTime}";
                                    scheduleSb.AppendLine($"- 시간: {timeStr}");
                                }

                                if (!string.IsNullOrEmpty(item.Content))
                                    scheduleSb.AppendLine($"- 내용: {item.Content}");

                                if (!string.IsNullOrEmpty(item.Location))
                                    scheduleSb.AppendLine($"- 장소: {item.Location}");

                                scheduleSb.AppendLine(); // 빈 줄
                            }
                        }

                        chunks.Add(new DocumentChunk
                        {
                            Content = scheduleSb.ToString(),
                            Metadata = new Dictionary<string, object>
                            {
                                { "type", "schedule_template" },
                                { "conventionId", convention.Id },
                                { "sourceType", "Schedule" },
                                { "template_id", template.Id },
                                { "template_title", template.CourseName }
                            }
                        });
                    }
                }
            }

            // 5. ConventionAction 정보 청크 (할 일 목록)
            if (data.ConventionActions != null && data.ConventionActions.Any())
            {
                var actionSb = new StringBuilder();
                actionSb.AppendLine($"# {convention.Title} 행사 필수 항목 및 할 일");
                actionSb.AppendLine($"- 총 {data.ConventionActions.Count}개의 항목이 있습니다.");
                actionSb.AppendLine();

                // 마감일 있는 항목 우선
                var sortedActions = data.ConventionActions
                    .OrderBy(a => a.Deadline.HasValue ? 0 : 1)
                    .ThenBy(a => a.Deadline)
                    .ThenBy(a => a.OrderNum);

                foreach (var action in sortedActions)
                {
                    actionSb.AppendLine($"## {action.Title}");

                    if (!string.IsNullOrEmpty(action.ActionType))
                        actionSb.AppendLine($"- 유형: {action.ActionType}");

                    if (!string.IsNullOrEmpty(action.Description))
                        actionSb.AppendLine($"- 설명: {action.Description}");

                    if (action.Deadline.HasValue)
                        actionSb.AppendLine($"- 마감: {action.Deadline:yyyy년 MM월 dd일 HH:mm}");

                    if (action.IsRequired)
                        actionSb.AppendLine("- ⚠️ 필수 항목입니다");

                    if (!string.IsNullOrEmpty(action.MapsTo))
                        actionSb.AppendLine($"- 경로: {action.MapsTo}");

                    actionSb.AppendLine(); // 빈 줄
                }

                chunks.Add(new DocumentChunk
                {
                    Content = actionSb.ToString(),
                    Metadata = new Dictionary<string, object>
                    {
                        { "type", "action_list" },
                        { "conventionId", convention.Id },
                        { "sourceType", "ConventionAction" }
                    }
                });
            }

            return Task.FromResult(chunks);
        }
    }
}
            