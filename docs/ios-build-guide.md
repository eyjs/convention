# StarTour iOS 앱 빌드 가이드 (Mac Studio)

## 사전 조건
- Mac Studio (macOS)
- Apple ID (무료 개인 계정도 가능)
- iPhone 실기기 (선택사항, 시뮬레이터로도 테스트 가능)

---

## 1단계: Xcode 설치

1. **App Store** 열기
2. **Xcode** 검색 → 설치 (약 12GB, 시간 좀 걸림)
3. 설치 완료 후 Xcode를 한번 실행
4. 라이선스 동의 + **Command Line Tools** 설치 팝업 → 동의/설치

---

## 2단계: Node.js 설치

터미널 열고 확인:
```bash
node -v
```

없으면 설치:
```bash
# Homebrew로 설치 (Homebrew 없으면 https://brew.sh 에서 먼저 설치)
brew install node
```

---

## 3단계: 프로젝트 클론

```bash
cd ~/dev    # 원하는 폴더로
git clone https://github.com/eyjs/convention.git
cd convention/ClientApp
```

이미 클론한 적 있으면:
```bash
cd ~/dev/convention    # 기존 폴더로
git pull
cd ClientApp
```

---

## 4단계: 프론트엔드 의존성 설치

```bash
npm install
```

---

## 5단계: iOS 프로젝트 생성 & 동기화

```bash
# iOS 네이티브 프로젝트 생성 (최초 1회)
npx cap add ios

# 웹 에셋 + Capacitor 플러그인 동기화
npx cap sync ios
```

> `ios/` 폴더가 이미 있다는 에러가 나면 삭제 후 다시:
> ```bash
> rm -rf ios
> npx cap add ios
> npx cap sync ios
> ```

---

## 6단계: Xcode에서 프로젝트 열기

```bash
npx cap open ios
```

또는 직접 열기: Xcode → File → Open → `ClientApp/ios/App/App.xcworkspace`

---

## 7단계: 서명 설정 (Signing)

1. Xcode 왼쪽 네비게이터에서 **App** (파란 아이콘) 클릭
2. 가운데 **Signing & Capabilities** 탭 선택
3. **Team** 드롭다운 → **Add Account** → Apple ID로 로그인
4. Team 선택 (본인 이름 - Personal Team)
5. **Bundle Identifier**: `kr.co.ifa.event` 확인
6. 에러 없으면 OK

> 무료 Apple ID는 7일마다 재서명 필요. 앱스토어 배포하려면 Apple Developer Program ($99/년) 가입 필요.

---

## 8단계: 빌드 & 실행

### 시뮬레이터로 테스트
1. 상단 디바이스 선택 → **iPhone 16** 등 시뮬레이터 선택
2. **▶ Run** (또는 `Cmd + R`)
3. 시뮬레이터가 뜨고 StarTour 앱이 실행됨

### 실기기로 테스트
1. iPhone을 USB(또는 USB-C)로 Mac에 연결
2. iPhone에서 **신뢰** 팝업 → 신뢰
3. iPhone **설정 → 개인정보 보호 및 보안 → 개발자 모드** → 켜기 (재부팅됨)
4. Xcode 상단 디바이스 선택 → 연결된 iPhone 선택
5. **▶ Run** (또는 `Cmd + R`)
6. 첫 실행 시 iPhone에서 **설정 → 일반 → VPN 및 기기 관리** → 개발자 앱 신뢰

---

## 9단계: IPA 파일 추출 (선택사항)

다른 iPhone에 배포하고 싶으면:

1. Xcode → **Product → Archive**
2. Archive 완료 → **Distribute App**
3. **Development** 또는 **Ad Hoc** 선택
4. IPA 파일 추출

> Ad Hoc 배포는 Apple Developer Program ($99/년) 필요.

---

## 업데이트 시

프론트엔드(CSS/JS) 변경은 서버 배포만 하면 앱에 자동 반영됨 (재빌드 불필요).

네이티브 코드 변경 시에만 재빌드:
```bash
cd ~/dev/convention
git pull
cd ClientApp
npx cap sync ios
# Xcode에서 ▶ Run
```

---

## 트러블슈팅

### "Untrusted Developer" 에러 (실기기)
→ iPhone **설정 → 일반 → VPN 및 기기 관리** → 해당 개발자 신뢰

### "No signing certificate" 에러
→ Xcode → Signing & Capabilities → Team을 다시 선택하거나 Apple ID 재로그인

### CocoaPods 에러
```bash
sudo gem install cocoapods
cd ios/App
pod install
```

### "Command PhaseScriptExecution failed" 에러
```bash
cd ClientApp
npx cap sync ios
# Xcode에서 Product → Clean Build Folder (Cmd + Shift + K)
# 다시 ▶ Run
```
