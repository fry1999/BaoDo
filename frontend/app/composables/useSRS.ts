import type { SRSRating } from '~/types'

/**
 * SM-2 Spaced Repetition Algorithm
 * Returns next interval (days) and updated ease factor.
 */
export function useSRS() {
  function calculateNext(
    rating: SRSRating,
    repetitions: number,
    interval: number,
    easeFactor: number,
  ): { interval: number; easeFactor: number; repetitions: number } {
    const MIN_EASE = 1.3
    let nextEase = easeFactor + (0.1 - (5 - rating) * (0.08 + (5 - rating) * 0.02))
    if (nextEase < MIN_EASE) nextEase = MIN_EASE

    if (rating < 3) {
      return { interval: 1, easeFactor: nextEase, repetitions: 0 }
    }

    let nextInterval: number
    if (repetitions === 0) nextInterval = 1
    else if (repetitions === 1) nextInterval = 6
    else nextInterval = Math.round(interval * nextEase)

    return { interval: nextInterval, easeFactor: nextEase, repetitions: repetitions + 1 }
  }

  function getNextReviewDate(intervalDays: number): Date {
    const d = new Date()
    d.setDate(d.getDate() + intervalDays)
    return d
  }

  function ratingLabel(rating: SRSRating): string {
    const labels: Record<SRSRating, string> = {
      1: 'Quên rồi',
      2: 'Khó',
      3: 'Ổn',
      4: 'Dễ',
    }
    return labels[rating]
  }

  function ratingColor(rating: SRSRating): string {
    const colors: Record<SRSRating, string> = {
      1: 'bg-red-500 hover:bg-red-600',
      2: 'bg-orange-500 hover:bg-orange-600',
      3: 'bg-blue-500 hover:bg-blue-600',
      4: 'bg-green-500 hover:bg-green-600',
    }
    return colors[rating]
  }

  return { calculateNext, getNextReviewDate, ratingLabel, ratingColor }
}
