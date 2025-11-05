import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useThemeStore = defineStore('theme', () => {
  // 기본 테마 (iFA)
  const defaultTheme = {
    id: 'ifa',
    name: 'iFA',
    primary: {
      50: '240 253 249',
      100: '204 251 239',
      200: '153 246 224',
      300: '95 233 208',
      400: '45 212 191',
      500: '20 184 166',
      600: '13 148 136',
      700: '15 118 110',
      800: '17 94 89',
      900: '19 78 74',
    },
  }

  // 사전 정의된 테마들
  const themes = ref({
    ifa: {
      id: 'ifa',
      name: 'iFA (Teal)',
      primary: {
        50: '240 253 249',
        100: '204 251 239',
        200: '153 246 224',
        300: '95 233 208',
        400: '45 212 191',
        500: '20 184 166',
        600: '13 148 136',
        700: '15 118 110',
        800: '17 94 89',
        900: '19 78 74',
      },
    },
    blue: {
      id: 'blue',
      name: 'Blue Ocean',
      primary: {
        50: '239 246 255',
        100: '219 234 254',
        200: '191 219 254',
        300: '147 197 253',
        400: '96 165 250',
        500: '59 130 246',
        600: '37 99 235',
        700: '29 78 216',
        800: '30 64 175',
        900: '30 58 138',
      },
    },
    purple: {
      id: 'purple',
      name: 'Purple Dream',
      primary: {
        50: '250 245 255',
        100: '243 232 255',
        200: '233 213 255',
        300: '216 180 254',
        400: '192 132 252',
        500: '168 85 247',
        600: '147 51 234',
        700: '126 34 206',
        800: '107 33 168',
        900: '88 28 135',
      },
    },
    green: {
      id: 'green',
      name: 'Forest Green',
      primary: {
        50: '240 253 244',
        100: '220 252 231',
        200: '187 247 208',
        300: '134 239 172',
        400: '74 222 128',
        500: '34 197 94',
        600: '22 163 74',
        700: '21 128 61',
        800: '22 101 52',
        900: '20 83 45',
      },
    },
    orange: {
      id: 'orange',
      name: 'Sunset Orange',
      primary: {
        50: '255 247 237',
        100: '255 237 213',
        200: '254 215 170',
        300: '253 186 116',
        400: '251 146 60',
        500: '249 115 22',
        600: '234 88 12',
        700: '194 65 12',
        800: '154 52 18',
        900: '124 45 18',
      },
    },
    pink: {
      id: 'pink',
      name: 'Cherry Pink',
      primary: {
        50: '253 242 248',
        100: '252 231 243',
        200: '251 207 232',
        300: '249 168 212',
        400: '244 114 182',
        500: '236 72 153',
        600: '219 39 119',
        700: '190 24 93',
        800: '157 23 77',
        900: '131 24 67',
      },
    },
    red: {
      id: 'red',
      name: 'Ruby Red',
      primary: {
        50: '254 242 242',
        100: '254 226 226',
        200: '254 202 202',
        300: '252 165 165',
        400: '248 113 113',
        500: '239 68 68',
        600: '220 38 38',
        700: '185 28 28',
        800: '153 27 27',
        900: '127 29 29',
      },
    },
  })

  const currentTheme = ref(defaultTheme)

  // CSS 변수 적용
  function applyCSSVariables(theme) {
    const root = document.documentElement

    Object.entries(theme.primary).forEach(([shade, color]) => {
      root.style.setProperty(`--color-primary-${shade}`, color)
    })
  }

  // 테마 변경
  function setTheme(themeId) {
    const theme = themes.value[themeId]
    if (theme) {
      currentTheme.value = theme
      applyCSSVariables(theme)
      localStorage.setItem('convention-theme', themeId)
    }
  }

  // Convention별 테마 설정
  function setThemeByConvention(conventionId, customTheme) {
    if (customTheme) {
      // 커스텀 컬러가 있으면 즉시 적용
      const theme = {
        id: `custom-${conventionId}`,
        name: 'Custom Theme',
        primary: customTheme,
      }
      currentTheme.value = theme
      applyCSSVariables(theme)
    } else {
      // 없으면 기본 테마
      setTheme('ifa')
    }
  }

  // HEX 색상을 RGB로 변환
  function hexToRgb(hex) {
    const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex)
    return result
      ? {
          r: parseInt(result[1], 16),
          g: parseInt(result[2], 16),
          b: parseInt(result[3], 16),
        }
      : null
  }

  // HEX 색상을 기반으로 모든 shade 생성
  function generateThemeFromColor(hexColor) {
    const rgb = hexToRgb(hexColor)
    if (!rgb) return null

    // 간단한 구현 - 밝기 조절로 shade 생성
    const shades = {
      50: lightenRgb(rgb, 0.95),
      100: lightenRgb(rgb, 0.85),
      200: lightenRgb(rgb, 0.7),
      300: lightenRgb(rgb, 0.5),
      400: lightenRgb(rgb, 0.25),
      500: `${rgb.r} ${rgb.g} ${rgb.b}`,
      600: darkenRgb(rgb, 0.15),
      700: darkenRgb(rgb, 0.3),
      800: darkenRgb(rgb, 0.45),
      900: darkenRgb(rgb, 0.6),
    }

    return shades
  }

  // RGB 밝게
  function lightenRgb(rgb, amount) {
    const r = Math.round(rgb.r + (255 - rgb.r) * amount)
    const g = Math.round(rgb.g + (255 - rgb.g) * amount)
    const b = Math.round(rgb.b + (255 - rgb.b) * amount)
    return `${r} ${g} ${b}`
  }

  // RGB 어둡게
  function darkenRgb(rgb, amount) {
    const r = Math.round(rgb.r * (1 - amount))
    const g = Math.round(rgb.g * (1 - amount))
    const b = Math.round(rgb.b * (1 - amount))
    return `${r} ${g} ${b}`
  }

  // 초기화
  function init() {
    const savedTheme = localStorage.getItem('convention-theme')
    if (savedTheme && themes.value[savedTheme]) {
      setTheme(savedTheme)
    } else {
      applyCSSVariables(defaultTheme)
    }
  }

  return {
    themes,
    currentTheme,
    setTheme,
    setThemeByConvention,
    generateThemeFromColor,
    init,
  }
})
