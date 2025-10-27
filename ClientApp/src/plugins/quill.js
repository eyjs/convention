import { Quill } from '@vueup/vue-quill'
import BlotFormatter from 'quill-blot-formatter'

export function setupQuill() {
  if (!Quill.imports['modules/blotFormatter']) {
    Quill.register('modules/blotFormatter', BlotFormatter)
  }
}
