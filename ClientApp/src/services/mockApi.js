// Mock API - 백엔드 없이 프론트엔드 개발/테스트용
// 실제 API가 준비되면 이 파일은 사용하지 않음

export const mockAPI = {
  // 대시보드 통계
  getConventionStats: (conventionId) => {
    return {
      totalGuests: 45,
      totalSchedules: 8,
      scheduleAssignments: 120,
      recentGuests: [
        {
          id: 1,
          guestName: '김철수',
          corpPart: '기획팀',
          telephone: '010-1234-5678',
        },
        {
          id: 2,
          guestName: '이영희',
          corpPart: '영업팀',
          telephone: '010-2345-6789',
        },
        {
          id: 3,
          guestName: '박지성',
          corpPart: 'IT팀',
          telephone: '010-3456-7890',
        },
        {
          id: 4,
          guestName: '최민수',
          corpPart: '인사팀',
          telephone: '010-4567-8901',
        },
        {
          id: 5,
          guestName: '정수연',
          corpPart: '재무팀',
          telephone: '010-5678-9012',
        },
      ],
      scheduleStats: [
        { id: 1, courseName: '바티칸 투어', itemCount: 4, guestCount: 15 },
        { id: 2, courseName: '콜로세움 관람', itemCount: 3, guestCount: 20 },
        { id: 3, courseName: '트레비 분수 방문', itemCount: 2, guestCount: 12 },
        {
          id: 4,
          courseName: '스페인 광장 자유시간',
          itemCount: 1,
          guestCount: 25,
        },
        { id: 5, courseName: '파스타 요리 체험', itemCount: 5, guestCount: 18 },
      ],
      attributeStats: [
        {
          attributeKey: '호차',
          totalCount: 45,
          values: [
            { value: '1호차', count: 15 },
            { value: '2호차', count: 18 },
            { value: '3호차', count: 12 },
          ],
        },
        {
          attributeKey: '식사제한',
          totalCount: 8,
          values: [
            { value: '채식', count: 3 },
            { value: '할랄', count: 2 },
            { value: '알러지(견과류)', count: 2 },
            { value: '알러지(해산물)', count: 1 },
          ],
        },
        {
          attributeKey: '객실타입',
          totalCount: 45,
          values: [
            { value: '싱글', count: 20 },
            { value: '트윈', count: 22 },
            { value: '스위트', count: 3 },
          ],
        },
      ],
    }
  },

  // 참석자 목록
  getGuests: (conventionId) => {
    return [
      {
        id: 1,
        guestName: '김철수',
        telephone: '010-1234-5678',
        corpPart: '기획팀',
        residentNumber: '850315-1******',
        affiliation: 'A그룹',
        scheduleTemplates: [
          { scheduleTemplateId: 1, courseName: '바티칸 투어' },
          { scheduleTemplateId: 2, courseName: '콜로세움 관람' },
        ],
        attributes: [
          { attributeKey: '호차', attributeValue: '1호차' },
          { attributeKey: '객실타입', attributeValue: '트윈' },
        ],
      },
      {
        id: 2,
        guestName: '이영희',
        telephone: '010-2345-6789',
        corpPart: '영업팀',
        residentNumber: '920815-2******',
        affiliation: 'B그룹',
        scheduleTemplates: [
          { scheduleTemplateId: 1, courseName: '바티칸 투어' },
          { scheduleTemplateId: 3, courseName: '트레비 분수 방문' },
        ],
        attributes: [
          { attributeKey: '호차', attributeValue: '2호차' },
          { attributeKey: '식사제한', attributeValue: '채식' },
        ],
      },
      {
        id: 3,
        guestName: '박지성',
        telephone: '010-3456-7890',
        corpPart: 'IT팀',
        residentNumber: '880520-1******',
        affiliation: 'A그룹',
        scheduleTemplates: [],
        attributes: [],
      },
    ]
  },

  // 일정 템플릿 목록
  getScheduleTemplates: (conventionId) => {
    return [
      {
        id: 1,
        courseName: '바티칸 투어',
        description: '바티칸 박물관 및 시스티나 성당 관람',
        orderNum: 0,
        guestCount: 15,
        scheduleItems: [
          {
            id: 101,
            scheduleTemplateId: 1,
            title: '바티칸 박물관 입장',
            scheduleDate: '2024-11-15T09:00:00',
            startTime: '09:00',
            location: '바티칸 박물관',
            content: '가이드 투어 진행',
          },
          {
            id: 102,
            scheduleTemplateId: 1,
            title: '시스티나 성당',
            scheduleDate: '2024-11-15T11:00:00',
            startTime: '11:00',
            location: '시스티나 성당',
            content: '미켈란젤로 천장화 감상',
          },
        ],
      },
      {
        id: 2,
        courseName: '콜로세움 관람',
        description: '고대 로마 원형 경기장 투어',
        orderNum: 1,
        guestCount: 20,
        scheduleItems: [
          {
            id: 201,
            scheduleTemplateId: 2,
            title: '콜로세움 입장',
            scheduleDate: '2024-11-15T14:00:00',
            startTime: '14:00',
            location: '콜로세움',
            content: '검투사 경기장 역사 설명',
          },
        ],
      },
      {
        id: 3,
        courseName: '트레비 분수 방문',
        description: '로마의 아름다운 바로크 분수',
        orderNum: 2,
        guestCount: 12,
        scheduleItems: [
          {
            id: 301,
            scheduleTemplateId: 3,
            title: '트레비 분수',
            scheduleDate: '2024-11-15T16:30:00',
            startTime: '16:30',
            location: '트레비 분수',
            content: '동전 던지기 및 사진 촬영',
          },
        ],
      },
    ]
  },

  // 참석자 상세 정보
  getGuestDetail: (guestId) => {
    return {
      id: guestId,
      guestName: '김철수',
      telephone: '010-1234-5678',
      corpPart: '기획팀',
      residentNumber: '850315-1******',
      affiliation: 'A그룹',
      attributes: {
        호차: '1호차',
        객실타입: '트윈',
        특이사항: '없음',
      },
      schedules: [
        {
          scheduleTemplateId: 1,
          courseName: '바티칸 투어',
          description: '바티칸 박물관 및 시스티나 성당 관람',
          items: [
            {
              id: 101,
              title: '바티칸 박물관 입장',
              scheduleDate: '2024-11-15T09:00:00',
              startTime: '09:00',
              location: '바티칸 박물관',
              content: '가이드 투어 진행',
            },
            {
              id: 102,
              title: '시스티나 성당',
              scheduleDate: '2024-11-15T11:00:00',
              startTime: '11:00',
              location: '시스티나 성당',
              content: '미켈란젤로 천장화 감상',
            },
          ],
        },
      ],
    }
  },

  // 속성 템플릿
  getAttributeTemplates: (conventionId) => {
    return [
      {
        id: 1,
        conventionId,
        attributeKey: '호차',
        displayName: '버스 호차',
        dataType: 'select',
        isRequired: true,
        attributeValues: '["1호차", "2호차", "3호차"]',
        orderNum: 0,
      },
      {
        id: 2,
        conventionId,
        attributeKey: '객실타입',
        displayName: '호텔 객실',
        dataType: 'select',
        isRequired: true,
        attributeValues: '["싱글", "트윈", "더블", "스위트"]',
        orderNum: 1,
      },
      {
        id: 3,
        conventionId,
        attributeKey: '식사제한',
        displayName: '식사 제한사항',
        dataType: 'select',
        isRequired: false,
        attributeValues:
          '["없음", "채식", "할랄", "알러지(견과류)", "알러지(해산물)", "기타"]',
        orderNum: 2,
      },
      {
        id: 4,
        conventionId,
        attributeKey: '특이사항',
        displayName: '특이사항',
        dataType: 'text',
        isRequired: false,
        attributeValues: null,
        orderNum: 3,
      },
    ]
  },

  // Convention 정보
  getConvention: (id) => {
    return {
      id,
      title: '2024 로마 워크숍',
      conventionType: 'OVERSEAS',
      startDate: '2024-11-15',
      endDate: '2024-11-20',
      description: '로마에서 진행되는 연례 워크숍',
    }
  },
}

// 개발 모드 여부 확인
export const isDevelopmentMode = () => {
  return import.meta.env.DEV && import.meta.env.VITE_USE_MOCK === 'true'
}
