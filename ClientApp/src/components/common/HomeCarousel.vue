<template>
  <div v-if="banners.length > 0" class="rounded-2xl overflow-hidden shadow-lg">
    <swiper
      :modules="modules"
      :autoplay="{ delay: 4000, disableOnInteraction: false }"
      :pagination="{ clickable: true }"
      :loop="banners.length > 1"
      class="home-carousel"
    >
      <swiper-slide v-for="banner in banners" :key="banner.id">
        <div class="relative cursor-pointer" @click="onBannerClick(banner)">
          <img
            loading="lazy"
            :src="banner.imageUrl"
            :alt="banner.title || '배너'"
            class="w-full h-44 object-cover"
          />
          <button
            v-if="
              (banner.linkUrl || banner.detailImagesJson) && banner.linkLabel
            "
            class="absolute bottom-4 right-4 px-4 py-2 bg-white/90 backdrop-blur-sm text-gray-900 text-sm font-semibold rounded-full shadow hover:bg-white transition-colors"
            @click.stop="onBannerClick(banner)"
          >
            {{ banner.linkLabel }}
            <svg
              class="w-3.5 h-3.5 inline-block ml-1"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 5l7 7-7 7"
              />
            </svg>
          </button>
        </div>
      </swiper-slide>
    </swiper>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { Swiper, SwiperSlide } from 'swiper/vue'
import { Autoplay, Pagination } from 'swiper/modules'
import 'swiper/css'
import 'swiper/css/pagination'

const modules = [Autoplay, Pagination]

defineProps({
  banners: { type: Array, default: () => [] },
})

const router = useRouter()

function onBannerClick(banner) {
  if (banner.detailImagesJson) {
    router.push(`/banners/${banner.id}`)
    return
  }
  if (!banner.linkUrl) return
  if (banner.linkUrl.startsWith('http')) {
    window.open(banner.linkUrl, '_blank', 'noopener,noreferrer')
  } else {
    router.push(banner.linkUrl)
  }
}
</script>

<style>
.home-carousel .swiper-pagination-bullet {
  width: 6px;
  height: 6px;
  background: rgba(255, 255, 255, 0.5);
  opacity: 1;
}
.home-carousel .swiper-pagination-bullet-active {
  width: 18px;
  border-radius: 3px;
  background: #fff;
}
</style>
