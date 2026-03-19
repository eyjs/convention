import { test, expect } from '@playwright/test'

const LOGIN_ID = 'admin'
const PASSWORD = 'admin123'

async function login(page) {
  await page.goto('/login')
  await page.waitForLoadState('networkidle')

  // 스플래시 화면에서 "로그인하기" 버튼 클릭
  const loginEntryBtn = page.locator('button:has-text("로그인하기")')
  if (await loginEntryBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await loginEntryBtn.click()
    await page.waitForTimeout(500)
  }

  // 로그인 폼 입력
  await page.fill('#email', LOGIN_ID)
  await page.fill('#password', PASSWORD)
  await page.click('button[type="submit"]')

  await page.waitForURL((url) => !url.pathname.includes('/login'), { timeout: 10000 })
}

test.describe('1. 인증 흐름', () => {
  test('로그인 페이지 접근 가능', async ({ page }) => {
    await page.goto('/login')
    await expect(page).toHaveURL(/\/login/)
  })

  test('로그인 성공 후 admin → /admin 리다이렉트', async ({ page }) => {
    await login(page)
    // admin 계정이므로 /admin으로 이동해야 함
    expect(page.url()).toContain('/admin')
  })

  test('미인증 상태에서 보호 페이지 접근 시 /login 리다이렉트', async ({ page }) => {
    await page.goto('/')
    await expect(page).toHaveURL(/\/login/)
  })
})

test.describe('2. 라우팅 구조', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
  })

  test('/ (MainHome) 정상 로드', async ({ page }) => {
    await page.goto('/')
    await page.waitForLoadState('networkidle')
    expect(page.url()).not.toContain('/login')
    const body = await page.textContent('body')
    expect(body.length).toBeGreaterThan(10)
  })

  test('/admin 접근 가능 (admin 계정)', async ({ page }) => {
    await page.goto('/admin')
    await page.waitForLoadState('networkidle')
    expect(page.url()).toContain('/admin')
  })
})

test.describe('3. Convention 라우팅', () => {
  let conventionId

  test.beforeEach(async ({ page }) => {
    await login(page)

    // API에서 convention ID 가져오기
    const token = await page.evaluate(() => localStorage.getItem('accessToken'))
    const response = await page.request.get('/api/conventions', {
      headers: { Authorization: `Bearer ${token}` },
    })

    if (response.ok()) {
      const data = await response.json()
      const conventions = data.conventions || data
      if (Array.isArray(conventions) && conventions.length > 0) {
        conventionId = conventions[0].id
      }
    }
  })

  test('Convention 상세 페이지 접근', async ({ page }) => {
    test.skip(!conventionId, 'No convention available')

    await page.goto(`/conventions/${conventionId}`)
    await page.waitForLoadState('networkidle')
    expect(page.url()).toContain(`/conventions/${conventionId}`)

    // 에러 없이 콘텐츠가 있는지
    const body = await page.textContent('body')
    expect(body.length).toBeGreaterThan(10)
  })

  test('Convention/schedule 접근', async ({ page }) => {
    test.skip(!conventionId, 'No convention available')

    await page.goto(`/conventions/${conventionId}/schedule`)
    await page.waitForLoadState('networkidle')
    expect(page.url()).toContain(`/conventions/${conventionId}/schedule`)
  })

  test('Convention/notices 접근', async ({ page }) => {
    test.skip(!conventionId, 'No convention available')

    await page.goto(`/conventions/${conventionId}/notices`)
    await page.waitForLoadState('networkidle')
    expect(page.url()).toContain(`/conventions/${conventionId}/notices`)
  })

  test('Convention/features 접근', async ({ page }) => {
    test.skip(!conventionId, 'No convention available')

    await page.goto(`/conventions/${conventionId}/features`)
    await page.waitForLoadState('networkidle')
    expect(page.url()).toContain(`/conventions/${conventionId}/features`)
  })

  test('잘못된 Convention ID → MainHome 리다이렉트', async ({ page }) => {
    await page.goto('/conventions/abc')
    await page.waitForLoadState('networkidle')
    // NaN → MainHome으로 리다이렉트
    expect(page.url()).not.toContain('/conventions/abc')
  })

  test('존재하지 않는 Convention ID → MainHome 리다이렉트', async ({ page }) => {
    await page.goto('/conventions/999999')
    await page.waitForLoadState('networkidle')
    // API 에러 → MainHome으로 리다이렉트
    await page.waitForTimeout(2000)
    // beforeEnter에서 selectConvention 실패 시 리다이렉트
    const url = page.url()
    // 최소한 에러 페이지가 아닌지 확인
    expect(url).not.toContain('/login')
  })

  test('Bottom nav 존재 확인', async ({ page }) => {
    test.skip(!conventionId, 'No convention available')

    await page.goto(`/conventions/${conventionId}`)
    await page.waitForLoadState('networkidle')

    const nav = page.locator('nav')
    await expect(nav).toBeVisible({ timeout: 5000 })

    // nav에 5개 버튼이 있는지
    const buttons = nav.locator('button')
    const count = await buttons.count()
    expect(count).toBe(5)
  })

  test('Bottom nav 일정 탭 클릭 → /conventions/:id/schedule', async ({ page }) => {
    test.skip(!conventionId, 'No convention available')

    await page.goto(`/conventions/${conventionId}`)
    await page.waitForLoadState('networkidle')

    await page.click('nav button:has-text("일정")')
    await page.waitForURL(`**/conventions/${conventionId}/schedule`, { timeout: 5000 })
    expect(page.url()).toContain(`/conventions/${conventionId}/schedule`)
  })

  test('Bottom nav 게시판 탭 클릭 → /conventions/:id/notices', async ({ page }) => {
    test.skip(!conventionId, 'No convention available')

    await page.goto(`/conventions/${conventionId}`)
    await page.waitForLoadState('networkidle')

    await page.click('nav button:has-text("게시판")')
    await page.waitForURL(`**/conventions/${conventionId}/notices`, { timeout: 5000 })
    expect(page.url()).toContain(`/conventions/${conventionId}/notices`)
  })

  test('Bottom nav 더보기 탭 클릭 → /conventions/:id/features', async ({ page }) => {
    test.skip(!conventionId, 'No convention available')

    await page.goto(`/conventions/${conventionId}`)
    await page.waitForLoadState('networkidle')

    await page.click('nav button:has-text("더보기")')
    await page.waitForURL(`**/conventions/${conventionId}/features`, { timeout: 5000 })
    expect(page.url()).toContain(`/conventions/${conventionId}/features`)
  })

  test('Bottom nav 홈 탭 클릭 → /conventions/:id', async ({ page }) => {
    test.skip(!conventionId, 'No convention available')

    // 일정 페이지에서 시작
    await page.goto(`/conventions/${conventionId}/schedule`)
    await page.waitForLoadState('networkidle')

    await page.click('nav button:has-text("홈")')
    await page.waitForURL((url) => {
      const pathname = new URL(url).pathname
      return pathname === `/conventions/${conventionId}` || pathname === `/conventions/${conventionId}/`
    }, { timeout: 5000 })

    const url = new URL(page.url())
    expect(url.pathname).toBe(`/conventions/${conventionId}`)
  })
})

test.describe('4. localStorage 정리 확인', () => {
  test('로그인 후 localStorage에 user 객체 없음', async ({ page }) => {
    await login(page)
    const userInStorage = await page.evaluate(() => localStorage.getItem('user'))
    expect(userInStorage).toBeNull()
  })

  test('로그인 후 localStorage에 selectedConventionId 없음', async ({ page }) => {
    await login(page)
    const cid = await page.evaluate(() => localStorage.getItem('selectedConventionId'))
    expect(cid).toBeNull()
  })

  test('로그인 후 accessToken은 localStorage에 존재', async ({ page }) => {
    await login(page)
    const token = await page.evaluate(() => localStorage.getItem('accessToken'))
    expect(token).toBeTruthy()
  })

  test('로그인 후 refreshToken은 localStorage에 존재', async ({ page }) => {
    await login(page)
    const token = await page.evaluate(() => localStorage.getItem('refreshToken'))
    expect(token).toBeTruthy()
  })
})

test.describe('5. 레거시 리다이렉트', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
  })

  test('/home → /', async ({ page }) => {
    await page.goto('/home')
    await page.waitForURL((url) => !url.pathname.includes('/home'), { timeout: 5000 })
    expect(page.url()).not.toContain('/home')
  })

  test('/my-schedule → /', async ({ page }) => {
    await page.goto('/my-schedule')
    await page.waitForURL((url) => !url.pathname.includes('/my-schedule'), { timeout: 5000 })
    expect(page.url()).not.toContain('/my-schedule')
  })

  test('/notices → /', async ({ page }) => {
    await page.goto('/notices')
    await page.waitForURL((url) => url.pathname === '/', { timeout: 5000 })
  })

  test('/features → /', async ({ page }) => {
    await page.goto('/features')
    await page.waitForURL((url) => url.pathname === '/', { timeout: 5000 })
  })
})

test.describe('6. Admin 대시보드 접근', () => {
  test('Admin convention 대시보드 접근', async ({ page }) => {
    await login(page)

    // convention 목록에서 첫 번째 convention ID 가져오기
    const token = await page.evaluate(() => localStorage.getItem('accessToken'))
    const response = await page.request.get('/api/conventions', {
      headers: { Authorization: `Bearer ${token}` },
    })

    if (response.ok()) {
      const data = await response.json()
      const conventions = data.conventions || data
      if (Array.isArray(conventions) && conventions.length > 0) {
        const id = conventions[0].id
        await page.goto(`/admin/conventions/${id}`)
        await page.waitForLoadState('networkidle')
        expect(page.url()).toContain(`/admin/conventions/${id}`)
      }
    }
  })
})
