import { Country, City } from 'country-state-city'

// 한국어 국가명 매핑
const countryNameKr = {
  KR: '대한민국',
  JP: '일본',
  CN: '중국',
  TW: '대만',
  HK: '홍콩',
  MO: '마카오',
  TH: '태국',
  VN: '베트남',
  SG: '싱가포르',
  MY: '말레이시아',
  ID: '인도네시아',
  PH: '필리핀',
  US: '미국',
  CA: '캐나다',
  MX: '멕시코',
  GB: '영국',
  FR: '프랑스',
  DE: '독일',
  IT: '이탈리아',
  ES: '스페인',
  PT: '포르투갈',
  NL: '네덜란드',
  BE: '벨기에',
  CH: '스위스',
  AT: '오스트리아',
  GR: '그리스',
  TR: '터키',
  CZ: '체코',
  PL: '폴란드',
  HU: '헝가리',
  HR: '크로아티아',
  AU: '호주',
  NZ: '뉴질랜드',
  FJ: '피지',
  AE: '아랍에미리트',
  EG: '이집트',
  ZA: '남아프리카공화국',
  MA: '모로코',
  IN: '인도',
  NP: '네팔',
  LK: '스리랑카',
  MV: '몰디브',
  RU: '러시아',
  BR: '브라질',
  AR: '아르헨티나',
  PE: '페루',
  CL: '칠레',
  CO: '콜롬비아',
  SE: '스웨덴',
  NO: '노르웨이',
  DK: '덴마크',
  FI: '핀란드',
  IS: '아이슬란드',
  IE: '아일랜드',
  LA: '라오스',
  KH: '캄보디아',
  MM: '미얀마',
}

// 한국어 도시명 매핑 (주요 관광도시)
const cityNameKr = {
  // 일본
  'Tokyo': '도쿄',
  'Osaka': '오사카',
  'Kyoto': '교토',
  'Fukuoka': '후쿠오카',
  'Sapporo': '삿포로',
  'Nagoya': '나고야',
  'Yokohama': '요코하마',
  'Kobe': '고베',
  'Nara': '나라',
  'Okinawa': '오키나와',
  'Hiroshima': '히로시마',
  'Sendai': '센다이',
  'Kanazawa': '가나자와',
  'Nagasaki': '나가사키',
  'Hakone': '하코네',

  // 중국
  'Beijing': '베이징',
  'Shanghai': '상하이',
  'Guangzhou': '광저우',
  'Shenzhen': '선전',
  'Chengdu': '청두',
  'Hangzhou': '항저우',
  'Xian': '시안',
  'Suzhou': '쑤저우',
  'Qingdao': '칭다오',
  'Harbin': '하얼빈',

  // 대만
  'Taipei': '타이베이',
  'Kaohsiung': '가오슝',
  'Taichung': '타이중',
  'Tainan': '타이난',
  'Hualien': '화롄',

  // 동남아
  'Bangkok': '방콕',
  'Phuket': '푸켓',
  'Chiang Mai': '치앙마이',
  'Pattaya': '파타야',
  'Krabi': '끄라비',
  'Ho Chi Minh City': '호치민',
  'Hanoi': '하노이',
  'Da Nang': '다낭',
  'Nha Trang': '나트랑',
  'Hoi An': '호이안',
  'Singapore': '싱가포르',
  'Kuala Lumpur': '쿠알라룸푸르',
  'Penang': '페낭',
  'Langkawi': '랑카위',
  'Bali': '발리',
  'Jakarta': '자카르타',
  'Manila': '마닐라',
  'Cebu City': '세부',
  'Boracay': '보라카이',
  'Siem Reap': '시엠립',
  'Phnom Penh': '프놈펜',
  'Vientiane': '비엔티안',
  'Luang Prabang': '루앙프라방',
  'Yangon': '양곤',

  // 미국
  'New York': '뉴욕',
  'Los Angeles': '로스앤젤레스',
  'San Francisco': '샌프란시스코',
  'Las Vegas': '라스베가스',
  'Honolulu': '호놀룰루',
  'Chicago': '시카고',
  'Miami': '마이애미',
  'Seattle': '시애틀',
  'Boston': '보스턴',
  'Washington': '워싱턴',
  'Orlando': '올랜도',
  'San Diego': '샌디에이고',

  // 캐나다
  'Vancouver': '밴쿠버',
  'Toronto': '토론토',
  'Montreal': '몬트리올',
  'Calgary': '캘거리',
  'Ottawa': '오타와',

  // 유럽
  'London': '런던',
  'Paris': '파리',
  'Rome': '로마',
  'Barcelona': '바르셀로나',
  'Madrid': '마드리드',
  'Berlin': '베를린',
  'Munich': '뮌헨',
  'Frankfurt': '프랑크푸르트',
  'Amsterdam': '암스테르담',
  'Brussels': '브뤼셀',
  'Vienna': '비엔나',
  'Prague': '프라하',
  'Budapest': '부다페스트',
  'Zurich': '취리히',
  'Geneva': '제네바',
  'Lisbon': '리스본',
  'Porto': '포르토',
  'Athens': '아테네',
  'Santorini': '산토리니',
  'Venice': '베니스',
  'Florence': '피렌체',
  'Milan': '밀라노',
  'Nice': '니스',
  'Dublin': '더블린',
  'Edinburgh': '에든버러',
  'Stockholm': '스톡홀름',
  'Copenhagen': '코펜하겐',
  'Oslo': '오슬로',
  'Helsinki': '헬싱키',
  'Reykjavik': '레이캬비크',
  'Warsaw': '바르샤바',
  'Krakow': '크라쿠프',
  'Zagreb': '자그레브',
  'Dubrovnik': '두브로브니크',
  'Istanbul': '이스탄불',
  'Moscow': '모스크바',
  'Saint Petersburg': '상트페테르부르크',

  // 호주/뉴질랜드
  'Sydney': '시드니',
  'Melbourne': '멜버른',
  'Brisbane': '브리즈번',
  'Gold Coast': '골드코스트',
  'Perth': '퍼스',
  'Cairns': '케언즈',
  'Auckland': '오클랜드',
  'Queenstown': '퀸스타운',
  'Wellington': '웰링턴',

  // 중동
  'Dubai': '두바이',
  'Abu Dhabi': '아부다비',

  // 인도/남아시아
  'New Delhi': '뉴델리',
  'Mumbai': '뭄바이',
  'Kathmandu': '카트만두',
  'Colombo': '콜롬보',
  'Male': '말레',

  // 한국 - 주요 도시
  'Seoul': '서울',
  'Busan': '부산',
  'Busan-gwangyeoksi': '부산',
  'Jeju': '제주',
  'Jeju City': '제주',
  'Jeju-si': '제주',
  'Incheon': '인천',
  'Incheon-gwangyeoksi': '인천',
  'Daegu': '대구',
  'Daegu-gwangyeoksi': '대구',
  'Daejeon': '대전',
  'Daejeon-gwangyeoksi': '대전',
  'Gwangju': '광주',
  'Gwangju-gwangyeoksi': '광주',
  'Ulsan': '울산',
  'Ulsan-gwangyeoksi': '울산',
  'Sejong': '세종',
  'Sejong-si': '세종',
  // 한국 - 관광지/소도시
  'Gyeongju': '경주',
  'Gyeongju-si': '경주',
  'Gangneung': '강릉',
  'Gangneung-si': '강릉',
  'Sokcho': '속초',
  'Sokcho-si': '속초',
  'Jeonju': '전주',
  'Jeonju-si': '전주',
  'Yeosu': '여수',
  'Yeosu-si': '여수',
  'Suwon': '수원',
  'Suwon-si': '수원',
  'Jeongseon': '정선',
  'Jeongseon-gun': '정선',
  'Pyeongchang': '평창',
  'Pyeongchang-gun': '평창',
  'Chuncheon': '춘천',
  'Chuncheon-si': '춘천',
  'Wonju': '원주',
  'Wonju-si': '원주',
  'Donghae': '동해',
  'Donghae-si': '동해',
  'Samcheok': '삼척',
  'Samcheok-si': '삼척',
  'Taebaek': '태백',
  'Taebaek-si': '태백',
  'Yangyang': '양양',
  'Yangyang-gun': '양양',
  'Goseong': '고성',
  'Goseong-gun': '고성',
  'Inje': '인제',
  'Inje-gun': '인제',
  'Hongcheon': '홍천',
  'Hongcheon-gun': '홍천',
  'Hoengseong': '횡성',
  'Hoengseong-gun': '횡성',
  'Cheorwon': '철원',
  'Cheorwon-gun': '철원',
  'Hwacheon': '화천',
  'Hwacheon-gun': '화천',
  'Yanggu': '양구',
  'Yanggu-gun': '양구',
  'Andong': '안동',
  'Andong-si': '안동',
  'Pohang': '포항',
  'Pohang-si': '포항',
  'Gimhae': '김해',
  'Gimhae-si': '김해',
  'Changwon': '창원',
  'Changwon-si': '창원',
  'Jinju': '진주',
  'Jinju-si': '진주',
  'Tongyeong': '통영',
  'Tongyeong-si': '통영',
  'Geoje': '거제',
  'Geoje-si': '거제',
  'Namhae': '남해',
  'Namhae-gun': '남해',
  'Hadong': '하동',
  'Hadong-gun': '하동',
  'Sancheong': '산청',
  'Sancheong-gun': '산청',
  'Hamyang': '함양',
  'Hamyang-gun': '함양',
  'Gurye': '구례',
  'Gurye-gun': '구례',
  'Suncheon': '순천',
  'Suncheon-si': '순천',
  'Gwangyang': '광양',
  'Gwangyang-si': '광양',
  'Mokpo': '목포',
  'Mokpo-si': '목포',
  'Naju': '나주',
  'Naju-si': '나주',
  'Damyang': '담양',
  'Damyang-gun': '담양',
  'Boseong': '보성',
  'Boseong-gun': '보성',
  'Goheung': '고흥',
  'Goheung-gun': '고흥',
  'Wando': '완도',
  'Wando-gun': '완도',
  'Jindo': '진도',
  'Jindo-gun': '진도',
  'Haenam': '해남',
  'Haenam-gun': '해남',
  'Gochang': '고창',
  'Gochang-gun': '고창',
  'Buan': '부안',
  'Buan-gun': '부안',
  'Gunsan': '군산',
  'Gunsan-si': '군산',
  'Iksan': '익산',
  'Iksan-si': '익산',
  'Gimje': '김제',
  'Gimje-si': '김제',
  'Namwon': '남원',
  'Namwon-si': '남원',
  'Muju': '무주',
  'Muju-gun': '무주',
  'Jinan': '진안',
  'Jinan-gun': '진안',
  'Jangsu': '장수',
  'Jangsu-gun': '장수',
  'Imsil': '임실',
  'Imsil-gun': '임실',
  'Sunchang': '순창',
  'Sunchang-gun': '순창',
  'Gongju': '공주',
  'Gongju-si': '공주',
  'Buyeo': '부여',
  'Buyeo-gun': '부여',
  'Boryeong': '보령',
  'Boryeong-si': '보령',
  'Seosan': '서산',
  'Seosan-si': '서산',
  'Taean': '태안',
  'Taean-gun': '태안',
  'Danyang': '단양',
  'Danyang-gun': '단양',
  'Jecheon': '제천',
  'Jecheon-si': '제천',
  'Chungju': '충주',
  'Chungju-si': '충주',
  'Yeongju': '영주',
  'Yeongju-si': '영주',
  'Bonghwa': '봉화',
  'Bonghwa-gun': '봉화',
  'Uljin': '울진',
  'Uljin-gun': '울진',
  'Yeongdeok': '영덕',
  'Yeongdeok-gun': '영덕',
  'Cheongdo': '청도',
  'Cheongdo-gun': '청도',
  'Gyeongsan': '경산',
  'Gyeongsan-si': '경산',
  'Yeongcheon': '영천',
  'Yeongcheon-si': '영천',
  'Mungyeong': '문경',
  'Mungyeong-si': '문경',
  'Sangju': '상주',
  'Sangju-si': '상주',
  'Gimcheon': '김천',
  'Gimcheon-si': '김천',
  'Gumi': '구미',
  'Gumi-si': '구미',
  'Chilgok': '칠곡',
  'Chilgok-gun': '칠곡',
  'Seongju': '성주',
  'Seongju-gun': '성주',
  'Goryeong': '고령',
  'Goryeong-gun': '고령',
  'Hapcheon': '합천',
  'Hapcheon-gun': '합천',
  'Uiseong': '의성',
  'Uiseong-gun': '의성',
  'Cheongsong': '청송',
  'Cheongsong-gun': '청송',
  'Yeongyang': '영양',
  'Yeongyang-gun': '영양',
  'Ulleung': '울릉',
  'Ulleung-gun': '울릉',
  'Paju': '파주',
  'Paju-si': '파주',
  'Goyang': '고양',
  'Goyang-si': '고양',
  'Gimpo': '김포',
  'Gimpo-si': '김포',
  'Bucheon': '부천',
  'Bucheon-si': '부천',
  'Ansan': '안산',
  'Ansan-si': '안산',
  'Siheung': '시흥',
  'Siheung-si': '시흥',
  'Gwangmyeong': '광명',
  'Gwangmyeong-si': '광명',
  'Anyang': '안양',
  'Anyang-si': '안양',
  'Gunpo': '군포',
  'Gunpo-si': '군포',
  'Uiwang': '의왕',
  'Uiwang-si': '의왕',
  'Seongnam': '성남',
  'Seongnam-si': '성남',
  'Hanam': '하남',
  'Hanam-si': '하남',
  'Gwacheon': '과천',
  'Gwacheon-si': '과천',
  'Yongin': '용인',
  'Yongin-si': '용인',
  'Hwaseong': '화성',
  'Hwaseong-si': '화성',
  'Osan': '오산',
  'Osan-si': '오산',
  'Pyeongtaek': '평택',
  'Pyeongtaek-si': '평택',
  'Anseong': '안성',
  'Anseong-si': '안성',
  'Icheon': '이천',
  'Icheon-si': '이천',
  'Yeoju': '여주',
  'Yeoju-si': '여주',
  'Gwangju-si': '광주시',
  'Yangpyeong': '양평',
  'Yangpyeong-gun': '양평',
  'Gapyeong': '가평',
  'Gapyeong-gun': '가평',
  'Pocheon': '포천',
  'Pocheon-si': '포천',
  'Dongducheon': '동두천',
  'Dongducheon-si': '동두천',
  'Uijeongbu': '의정부',
  'Uijeongbu-si': '의정부',
  'Yangju': '양주',
  'Yangju-si': '양주',
  'Namyangju': '남양주',
  'Namyangju-si': '남양주',
  'Guri': '구리',
  'Guri-si': '구리',

  // 홍콩/마카오
  'Hong Kong': '홍콩',
  'Macau': '마카오',
}

// 역방향 매핑 (한국어 -> 영어)
const cityNameEnFromKr = Object.entries(cityNameKr).reduce((acc, [en, kr]) => {
  acc[kr] = en
  return acc
}, {})

const countryNameEnFromKr = Object.entries(countryNameKr).reduce((acc, [code, kr]) => {
  acc[kr] = code
  return acc
}, {})

// 통화 코드 매핑
const currencyByCountry = {
  KR: 'KRW',
  JP: 'JPY',
  CN: 'CNY',
  TW: 'TWD',
  HK: 'HKD',
  MO: 'MOP',
  TH: 'THB',
  VN: 'VND',
  SG: 'SGD',
  MY: 'MYR',
  ID: 'IDR',
  PH: 'PHP',
  US: 'USD',
  CA: 'CAD',
  MX: 'MXN',
  GB: 'GBP',
  FR: 'EUR',
  DE: 'EUR',
  IT: 'EUR',
  ES: 'EUR',
  PT: 'EUR',
  NL: 'EUR',
  BE: 'EUR',
  AT: 'EUR',
  GR: 'EUR',
  IE: 'EUR',
  FI: 'EUR',
  CH: 'CHF',
  CZ: 'CZK',
  PL: 'PLN',
  HU: 'HUF',
  HR: 'EUR',
  TR: 'TRY',
  AU: 'AUD',
  NZ: 'NZD',
  AE: 'AED',
  EG: 'EGP',
  ZA: 'ZAR',
  IN: 'INR',
  NP: 'NPR',
  LK: 'LKR',
  MV: 'MVR',
  RU: 'RUB',
  BR: 'BRL',
  SE: 'SEK',
  NO: 'NOK',
  DK: 'DKK',
  IS: 'ISK',
  KH: 'KHR',
  LA: 'LAK',
  MM: 'MMK',
}

// 타임존 매핑
const timezoneByCountry = {
  KR: 'Asia/Seoul',
  JP: 'Asia/Tokyo',
  CN: 'Asia/Shanghai',
  TW: 'Asia/Taipei',
  HK: 'Asia/Hong_Kong',
  MO: 'Asia/Macau',
  TH: 'Asia/Bangkok',
  VN: 'Asia/Ho_Chi_Minh',
  SG: 'Asia/Singapore',
  MY: 'Asia/Kuala_Lumpur',
  ID: 'Asia/Jakarta',
  PH: 'Asia/Manila',
  US: 'America/New_York',
  CA: 'America/Toronto',
  MX: 'America/Mexico_City',
  GB: 'Europe/London',
  FR: 'Europe/Paris',
  DE: 'Europe/Berlin',
  IT: 'Europe/Rome',
  ES: 'Europe/Madrid',
  PT: 'Europe/Lisbon',
  NL: 'Europe/Amsterdam',
  BE: 'Europe/Brussels',
  CH: 'Europe/Zurich',
  AT: 'Europe/Vienna',
  GR: 'Europe/Athens',
  TR: 'Europe/Istanbul',
  CZ: 'Europe/Prague',
  PL: 'Europe/Warsaw',
  HU: 'Europe/Budapest',
  HR: 'Europe/Zagreb',
  AU: 'Australia/Sydney',
  NZ: 'Pacific/Auckland',
  AE: 'Asia/Dubai',
  EG: 'Africa/Cairo',
  IN: 'Asia/Kolkata',
  NP: 'Asia/Kathmandu',
  RU: 'Europe/Moscow',
  BR: 'America/Sao_Paulo',
  SE: 'Europe/Stockholm',
  NO: 'Europe/Oslo',
  DK: 'Europe/Copenhagen',
  FI: 'Europe/Helsinki',
  IS: 'Atlantic/Reykjavik',
  IE: 'Europe/Dublin',
  KH: 'Asia/Phnom_Penh',
  LA: 'Asia/Vientiane',
  MM: 'Asia/Yangon',
}

// 인기 관광 도시 (국가별)
const popularCitiesByCountry = {
  JP: ['Tokyo', 'Osaka', 'Kyoto', 'Fukuoka', 'Sapporo', 'Okinawa', 'Nagoya', 'Hiroshima'],
  TH: ['Bangkok', 'Phuket', 'Chiang Mai', 'Pattaya', 'Krabi'],
  VN: ['Ho Chi Minh City', 'Hanoi', 'Da Nang', 'Nha Trang', 'Hoi An'],
  TW: ['Taipei', 'Kaohsiung', 'Taichung', 'Tainan', 'Hualien'],
  CN: ['Beijing', 'Shanghai', 'Guangzhou', 'Shenzhen', 'Hangzhou', 'Chengdu'],
  US: ['New York', 'Los Angeles', 'San Francisco', 'Las Vegas', 'Honolulu', 'Chicago'],
  FR: ['Paris', 'Nice', 'Lyon', 'Marseille'],
  IT: ['Rome', 'Venice', 'Florence', 'Milan'],
  ES: ['Barcelona', 'Madrid', 'Seville'],
  GB: ['London', 'Edinburgh', 'Manchester'],
  DE: ['Berlin', 'Munich', 'Frankfurt'],
  AU: ['Sydney', 'Melbourne', 'Brisbane', 'Gold Coast', 'Cairns'],
  SG: ['Singapore'],
  HK: ['Hong Kong'],
  MY: ['Kuala Lumpur', 'Penang', 'Langkawi'],
  ID: ['Bali', 'Jakarta', 'Yogyakarta'],
  PH: ['Manila', 'Cebu City', 'Boracay'],
  KR: ['Seoul', 'Busan', 'Jeju', 'Gyeongju', 'Gangneung', 'Jeonju', 'Yeosu'],
}

/**
 * 도시 검색 함수
 * @param {string} keyword - 검색 키워드 (한국어 또는 영어)
 * @param {number} limit - 최대 결과 수
 * @returns {Array} 검색 결과
 */
export function searchCities(keyword, limit = 10) {
  if (!keyword || keyword.trim().length < 1) {
    return []
  }

  const query = keyword.trim()
  const queryLower = query.toLowerCase()
  const results = []
  const addedCities = new Set() // 중복 방지 (displayName 기준)

  // 1. 한국어 국가명으로 검색한 경우 -> 해당 국가의 인기 도시 반환 (부분 일치 지원)
  let countryCode = null

  // 한국어 국가명 부분 일치 검색
  for (const [code, koreanName] of Object.entries(countryNameKr)) {
    if (koreanName.includes(query)) {
      countryCode = code
      break
    }
  }

  if (countryCode) {
    const popularCities = popularCitiesByCountry[countryCode] || []
    const country = Country.getCountryByCode(countryCode)

    popularCities.forEach(cityName => {
      const cities = City.getCitiesOfCountry(countryCode)
      const city = cities?.find(c => c.name === cityName)
      if (city) {
        const formatted = formatCityResult(city, country)
        if (!addedCities.has(formatted.displayName)) {
          addedCities.add(formatted.displayName)
          results.push(formatted)
        }
      }
    })

    if (results.length > 0) {
      return results.slice(0, limit)
    }
  }

  // 2. 한국어 도시명으로 검색 (부분 일치 지원)
  const matchingKoreanCities = []
  for (const [englishName, koreanName] of Object.entries(cityNameKr)) {
    if (koreanName.includes(query)) {
      matchingKoreanCities.push({
        englishName,
        koreanName,
        isExact: koreanName === query
      })
    }
  }

  // 정확히 일치하는 것 우선 정렬
  matchingKoreanCities.sort((a, b) => {
    if (a.isExact && !b.isExact) return -1
    if (!a.isExact && b.isExact) return 1
    // 인기 관광지 우선
    const aPopular = isPopularCity(a.englishName)
    const bPopular = isPopularCity(b.englishName)
    if (aPopular && !bPopular) return -1
    if (!aPopular && bPopular) return 1
    return 0
  })

  if (matchingKoreanCities.length > 0) {
    const allCities = City.getAllCities()
    for (const { englishName } of matchingKoreanCities) {
      const matchedCities = allCities.filter(c => c.name === englishName)
      for (const city of matchedCities) {
        const country = Country.getCountryByCode(city.countryCode)
        if (country) {
          const formatted = formatCityResult(city, country)
          if (!addedCities.has(formatted.displayName)) {
            addedCities.add(formatted.displayName)
            results.push(formatted)
          }
        }
        if (results.length >= limit) break
      }
      if (results.length >= limit) break
    }

    if (results.length > 0) {
      return results.slice(0, limit)
    }
  }

  // 3. 영어 국가명으로 검색 (부분 일치 지원)
  const allCountries = Country.getAllCountries()
  const matchedCountry = allCountries.find(c =>
    c.name.toLowerCase().includes(queryLower)
  )
  if (matchedCountry) {
    const popularCities = popularCitiesByCountry[matchedCountry.isoCode] || []
    popularCities.forEach(cityName => {
      const cities = City.getCitiesOfCountry(matchedCountry.isoCode)
      const city = cities?.find(c => c.name === cityName)
      if (city) {
        const formatted = formatCityResult(city, matchedCountry)
        if (!addedCities.has(formatted.displayName)) {
          addedCities.add(formatted.displayName)
          results.push(formatted)
        }
      }
    })

    if (results.length > 0) {
      return results.slice(0, limit)
    }
  }

  // 4. 영어 도시명으로 검색 (부분 일치)
  const allCities = City.getAllCities()
  const matchedCities = allCities.filter(c =>
    c.name.toLowerCase().includes(queryLower)
  )

  // 정확히 일치하는 것 우선
  matchedCities.sort((a, b) => {
    const aExact = a.name.toLowerCase() === queryLower
    const bExact = b.name.toLowerCase() === queryLower
    if (aExact && !bExact) return -1
    if (!aExact && bExact) return 1

    // 인기 관광지 우선
    const aPopular = isPopularCity(a.name)
    const bPopular = isPopularCity(b.name)
    if (aPopular && !bPopular) return -1
    if (!aPopular && bPopular) return 1

    return 0
  })

  for (const city of matchedCities) {
    const country = Country.getCountryByCode(city.countryCode)
    if (country) {
      const formatted = formatCityResult(city, country)
      if (!addedCities.has(formatted.displayName)) {
        addedCities.add(formatted.displayName)
        results.push(formatted)
      }
    }
    if (results.length >= limit) break
  }

  return results
}

/**
 * 인기 관광 도시인지 확인
 */
function isPopularCity(cityName) {
  return Object.values(popularCitiesByCountry).flat().includes(cityName)
}

/**
 * 도시 결과 포맷팅
 */
function formatCityResult(city, country) {
  const nameKr = cityNameKr[city.name] || city.name
  const countryKr = countryNameKr[country.isoCode] || country.name

  return {
    name: nameKr,
    name_en: city.name,
    country: countryKr,
    country_en: country.name,
    countryCode: country.isoCode,
    displayName: `${nameKr}, ${countryKr}`,
    isDomestic: country.isoCode === 'KR',
    latitude: parseFloat(city.latitude) || null,
    longitude: parseFloat(city.longitude) || null,
    timezone: timezoneByCountry[country.isoCode] || null,
    currency: currencyByCountry[country.isoCode] || null,
    stateCode: city.stateCode || null,
  }
}

/**
 * 국가 목록 가져오기
 */
export function getAllCountries() {
  return Country.getAllCountries().map(country => ({
    code: country.isoCode,
    name: countryNameKr[country.isoCode] || country.name,
    name_en: country.name,
  }))
}

/**
 * 특정 국가의 인기 도시 가져오기
 */
export function getPopularCities(countryCode) {
  const popularCities = popularCitiesByCountry[countryCode] || []
  const country = Country.getCountryByCode(countryCode)
  if (!country) return []

  const cities = City.getCitiesOfCountry(countryCode) || []
  const addedNames = new Set()
  const results = []

  popularCities.forEach(cityName => {
    const city = cities.find(c => c.name === cityName)
    if (city) {
      const formatted = formatCityResult(city, country)
      if (!addedNames.has(formatted.displayName)) {
        addedNames.add(formatted.displayName)
        results.push(formatted)
      }
    }
  })

  return results
}

export default {
  searchCities,
  getAllCountries,
  getPopularCities,
}
