/**
 * 거리 계산 Composable
 *
 * Haversine 공식을 사용하여 두 지점 간 거리를 계산합니다.
 */

/**
 * 두 좌표 간의 거리를 계산 (Haversine 공식)
 * @param {number} lat1 - 첫 번째 지점의 위도
 * @param {number} lon1 - 첫 번째 지점의 경도
 * @param {number} lat2 - 두 번째 지점의 위도
 * @param {number} lon2 - 두 번째 지점의 경도
 * @returns {number} 두 지점 간의 거리 (km)
 */
function calculateDistance(lat1, lon1, lat2, lon2) {
  const nLat1 = Number(lat1);
  const nLon1 = Number(lon1);
  const nLat2 = Number(lat2);
  const nLon2 = Number(lon2);

  if (!isFinite(nLat1) || !isFinite(nLon1) || !isFinite(nLat2) || !isFinite(nLon2)) {
    return Number.MAX_VALUE;
  }

  const R = 6371 // 지구 반경 (km)
  const dLat = toRad(nLat2 - nLat1)
  const dLon = toRad(nLon2 - nLon1)

  const a =
    Math.sin(dLat / 2) * Math.sin(dLat / 2) +
    Math.cos(toRad(nLat1)) * Math.cos(toRad(nLat2)) *
    Math.sin(dLon / 2) * Math.sin(dLon / 2)

  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
  const distance = R * c

  return distance
}

function toRad(degrees) {
  return degrees * (Math.PI / 180)
}

/**
 * 거리를 보기 좋은 형식으로 포맷팅
 * @param {number} distance - 거리 (km)
 * @returns {string} 포맷팅된 거리 문자열
 */
function formatDistance(distance) {
  if (distance < 1) {
    return `${Math.round(distance * 1000)}m`
  }
  return `${distance.toFixed(1)}km`
}

/**
 * 일정 목록의 각 항목 간 거리 계산
 * @param {Array} items - 일정 항목 배열 (latitude, longitude 포함)
 * @returns {Array} 거리 정보가 추가된 배열
 */
function calculateItemDistances(items) {
  if (!items || items.length === 0) return []

  return items.map((item, index) => {
    let distanceToNext = null

    if (index < items.length - 1) {
      const nextItem = items[index + 1]

      // 둘 다 위치 정보가 있는 경우에만 거리 계산
      if (
        item.latitude && item.longitude &&
        nextItem.latitude && nextItem.longitude
      ) {
        const distance = calculateDistance(
          item.latitude,
          item.longitude,
          nextItem.latitude,
          nextItem.longitude
        )
        distanceToNext = {
          raw: distance,
          formatted: formatDistance(distance)
        }
      }
    }

    return {
      ...item,
      distanceToNext
    }
  })
}

/**
 * 최근접 이웃 알고리즘으로 일정 최적화
 * (Nearest Neighbor Algorithm - 간단한 TSP 해법)
 * @param {Array} items - 일정 항목 배열
 * @returns {Array} 최적화된 순서의 일정 배열
 */
function optimizeRouteByDistance(items) {
  if (!items || items.length <= 1) return items;

  const itemsWithLocation = items.filter(
    item => item.latitude && item.longitude
  );

  if (itemsWithLocation.length <= 1) return items;

  // The first item is the fixed reference point.
  const referenceItem = itemsWithLocation[0];

  // Calculate distances from the reference item to all other items.
  const otherItems = itemsWithLocation.slice(1).map(item => {
    const distance = calculateDistance(
      referenceItem.latitude,
      referenceItem.longitude,
      item.latitude,
      item.longitude
    );
    return { ...item, distanceToRef: distance }; // Temporarily store distance for sorting
  });

  // Sort other items by their distance to the reference item.
  otherItems.sort((a, b) => a.distanceToRef - b.distanceToRef);

  // Construct the final optimized list: reference item + sorted other items.
  const optimized = [referenceItem, ...otherItems];

  // Remove the temporary distanceToRef property before returning.
  optimized.forEach(item => delete item.distanceToRef);

  return optimized;
}

/**
 * 총 이동 거리 계산
 * @param {Array} items - 일정 항목 배열
 * @returns {Object} { total: number, formatted: string }
 */
function calculateTotalDistance(items) {
  if (!items || items.length < 2) {
    return { total: 0, formatted: '0km' }
  }

  let total = 0

  for (let i = 0; i < items.length - 1; i++) {
    const current = items[i]
    const next = items[i + 1]

    if (
      current.latitude && current.longitude &&
      next.latitude && next.longitude
    ) {
      total += calculateDistance(
        current.latitude,
        current.longitude,
        next.latitude,
        next.longitude
      )
    }
  }

  return {
    total,
    formatted: formatDistance(total)
  }
}

export function useDistance() {
  return {
    calculateDistance,
    formatDistance,
    calculateItemDistances,
    optimizeRouteByDistance,
    calculateTotalDistance
  }
}
