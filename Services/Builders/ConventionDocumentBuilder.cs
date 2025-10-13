using LocalRAG.Models;
using LocalRAG.Models.DTOs;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;


namespace LocalRAG.Services.Builders
{
    public class ConventionDocumentBuilder
    {
        private readonly ILogger<ConventionDocumentBuilder> _logger;

        public ConventionDocumentBuilder(ILogger<ConventionDocumentBuilder> logger)
        {
            _logger = logger;
        }

        public List<DocumentChunk> BuildDocumentChunks(
            Convention convention,
            Dictionary<int, List<ScheduleItem>> guestSchedulesMap)
        {
            var chunks = new List<DocumentChunk>();

            // 1. Convention Info Chunk
            var conventionInfoSb = new StringBuilder();
            conventionInfoSb.AppendLine($"# 행사 정보: {convention.Title}");
            conventionInfoSb.AppendLine($"- 행사 종류: {convention.ConventionType}");
            if (convention.StartDate.HasValue)
            {
                string formattedStartDate = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:yyyy년 MM월 dd일}", convention.StartDate.Value);
                conventionInfoSb.AppendLine("- 시작일: " + formattedStartDate);
            }
            if (convention.EndDate.HasValue)
            {
                string formattedEndDate = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:yyyy년 MM월 dd일}", convention.EndDate.Value);
                conventionInfoSb.AppendLine("- 종료일: " + formattedEndDate);
            }

            chunks.Add(new DocumentChunk
            {
                Content = conventionInfoSb.ToString(),
                Metadata = new Dictionary<string, object>
                {
                    { "type", "convention_info" },
                    { "convention_id", convention.Id },
                    { "title", convention.Title }
                }
            });

            // 2. Schedule Item Chunks
            foreach (var template in convention.ScheduleTemplates.OrderBy(t => t.OrderNum))
            {
                foreach (var item in template.ScheduleItems.OrderBy(i => i.ScheduleDate).ThenBy(i => i.StartTime))
                {
                    string formattedDate = item.ScheduleDate.ToString("yyyy-MM-dd");
                    string formattedTime = item.StartTime.ToString();

                    var scheduleItemSb = new StringBuilder();
                    scheduleItemSb.AppendLine($"# 행사 일정: {item.Title}");
                    scheduleItemSb.AppendLine($"- 일시: {formattedDate} {item.StartTime:HH:mm}");
                    scheduleItemSb.AppendLine($"- 장소: {item.Location}");
                    scheduleItemSb.AppendLine($"- 내용: {item.Title}");

                    var metadata = new Dictionary<string, object>
                    {
                        { "type", "schedule_item" },
                        { "convention_id", convention.Id },
                        { "schedule_item_id", item.Id },
                        { "title", item.Title },
                        { "time", formattedTime }
                    };
                    metadata.Add("date", formattedDate);

                    chunks.Add(new DocumentChunk
                    {
                        Content = scheduleItemSb.ToString(),
                        Metadata = metadata
                    });
                }
            }

            // 3. Guest related chunks
            if (convention.Guests is not null && convention.Guests.Any())
            {
                foreach (var guest in convention.Guests)
                {
                    // Guest Info Chunk
                    var guestInfoSb = new StringBuilder();
                    guestInfoSb.AppendLine($"# 참석자 정보: {guest.GuestName}");
                    guestInfoSb.AppendLine($"- 이름: {guest.GuestName}");
                    if (!string.IsNullOrEmpty(guest.CorpPart)) guestInfoSb.AppendLine($"- 부서: {guest.CorpPart}");
                    if (!string.IsNullOrEmpty(guest.Telephone)) guestInfoSb.AppendLine($"- 연락처: {guest.Telephone}");
                    if (!string.IsNullOrEmpty(guest.Email)) guestInfoSb.AppendLine($"- 이메일: {guest.Email}");
                    if (!string.IsNullOrEmpty(guest.Affiliation)) guestInfoSb.AppendLine($"- 소속: {guest.Affiliation}");

                    chunks.Add(new DocumentChunk
                    {
                        Content = guestInfoSb.ToString(),
                        Metadata = new Dictionary<string, object>
                        {
                            { "type", "guest_info" },
                            { "convention_id", convention.Id },
                            { "guest_id", guest.Id },
                            { "name", guest.GuestName }
                        }
                    });

                    // Guest Attribute Chunks
                    if (guest.GuestAttributes is not null)
                    {
                        foreach (var attr in guest.GuestAttributes)
                        {
                            var guestAttrSb = new StringBuilder();
                            guestAttrSb.AppendLine($"# 참석자 추가 정보: {guest.GuestName}");
                            guestAttrSb.AppendLine($"- {attr.AttributeKey}: {attr.AttributeValue}");

                            chunks.Add(new DocumentChunk
                            {
                                Content = guestAttrSb.ToString(),
                                Metadata = new Dictionary<string, object>
                                {
                                    { "type", "guest_attribute" },
                                    { "convention_id", convention.Id },
                                    { "guest_id", guest.Id },
                                    { "attribute_key", attr.AttributeKey },
                                    { "attribute_value", attr.AttributeValue }
                                }
                            });
                        }
                    }

                    // Guest-Schedule Association Chunks
                    if (guestSchedulesMap.TryGetValue(guest.Id, out var assignedItems))
                    {
                        foreach (var item in assignedItems)
                        {
                            var guestScheduleSb = new StringBuilder();
                            guestScheduleSb.AppendLine($"Guest {guest.GuestName} will attend the {item.Title} on {item.ScheduleDate:yyyy-MM-dd} at {item.StartTime:HH:mm}.");

                            chunks.Add(new DocumentChunk
                            {
                                Content = guestScheduleSb.ToString(),
                                Metadata = new Dictionary<string, object>
                                {
                                    { "type", "guest_schedule" },
                                    { "convention_id", convention.Id },
                                    { "guest_id", guest.Id },
                                    { "schedule_item_id", item.Id }
                                }
                            });
                        }
                    }
                }
            }

            return chunks;
        }
    }
}