// ─── Auth & User ────────────────────────────────────────────────────────────

export type UserRole = 'user' | 'admin' | 'content_editor' | 'support'
export type SubscriptionPlan = 'free' | 'basic' | 'pro'

export interface UserProfile {
  id: string
  email: string
  fullName: string
  avatarUrl?: string
  role: UserRole
  subscription: SubscriptionPlan
  targetScore: number
  examDate?: string
  streakCount: number
  xpTotal: number
  level: number
  createdAt: string
}

// ─── Vocabulary & SRS ────────────────────────────────────────────────────────

export type PartOfSpeech = 'noun' | 'verb' | 'adjective' | 'adverb' | 'preposition' | 'conjunction' | 'phrase'
export type VocabTopic = 'Business' | 'Finance' | 'Office' | 'Travel' | 'Technology' | 'Human Resources' | 'Marketing' | 'Legal' | 'General'
export type VocabLevel = 'basic' | 'intermediate' | 'advanced'
export type SRSCardStatus = 'new' | 'learning' | 'review' | 'mastered'

export interface Vocabulary {
  id: string
  word: string
  phonetic: string
  audioUrl?: string
  meaningVi: string
  meaningEn: string
  partOfSpeech: PartOfSpeech
  examples: string[]
  collocations: string[]
  topic: VocabTopic
  level: VocabLevel
  imageUrl?: string
}

export interface UserVocabularyCard {
  id: string
  userId: string
  vocabId: string
  vocabulary?: Vocabulary
  status: SRSCardStatus
  interval: number
  easeFactor: number
  repetitions: number
  nextReviewAt: string
  lastReviewedAt?: string
}

export type SRSRating = 1 | 2 | 3 | 4

// ─── Grammar ─────────────────────────────────────────────────────────────────

export type GrammarCategory =
  | 'tenses'
  | 'word_form'
  | 'prepositions'
  | 'conjunctions'
  | 'articles'
  | 'subject_verb_agreement'
  | 'relative_clauses'
  | 'conditionals'
  | 'passive_voice'
  | 'comparison'

export interface GrammarLesson {
  id: string
  title: string
  category: GrammarCategory
  content: string
  difficulty: 1 | 2 | 3
  questions: GrammarQuestion[]
  relatedPartIds: string[]
  estimatedMinutes: number
  publishedAt?: string
}

export interface GrammarQuestion {
  id: string
  lessonId: string
  questionText: string
  options: string[]
  correctIndex: number
  explanation: string
}

// ─── Questions & Practice ────────────────────────────────────────────────────

export type ToeicPart = 1 | 2 | 3 | 4 | 5 | 6 | 7
export type Difficulty = 'easy' | 'medium' | 'hard'
export type QuestionTag = 'word_form' | 'tense' | 'preposition' | 'conjunction' | 'vocabulary' | 'inference' | 'detail' | 'main_idea'

export interface Question {
  id: string
  part: ToeicPart
  difficulty: Difficulty
  tags: QuestionTag[]
  questionText?: string
  imageUrl?: string
  audioUrl?: string
  transcript?: string
  options: string[]
  correctIndex: number
  explanation: string
  passageId?: string
}

export interface Passage {
  id: string
  part: 6 | 7
  passageType: 'single' | 'double' | 'triple'
  title?: string
  content: string
  questions: Question[]
}

// ─── Dictation ───────────────────────────────────────────────────────────────

export type DictationSource = 'library' | 'youtube'
export type DictationLevel = 'beginner' | 'intermediate' | 'advanced'
export type DictationTopic = 'Business' | 'Conversation' | 'Announcement' | 'News' | 'Lecture'

export interface DictationContent {
  id: string
  title: string
  source: DictationSource
  youtubeId?: string
  audioUrl?: string
  level: DictationLevel
  topic: DictationTopic
  duration: number
  segments: DictationSegment[]
  createdAt: string
}

export interface DictationSegment {
  index: number
  start: number
  end: number
  text: string
}

export interface DictationProgress {
  id: string
  userId: string
  contentId: string
  segmentIndex: number
  userInput: string
  accuracy: number
  attempts: number
  completedAt?: string
}

export interface DictationCheckResult {
  accuracy: number
  words: Array<{
    word: string
    status: 'correct' | 'wrong' | 'missing' | 'extra'
  }>
}

// ─── Tests & Exam ────────────────────────────────────────────────────────────

export type TestType = 'full' | 'mini' | 'part'

export interface Test {
  id: string
  title: string
  type: TestType
  totalQuestions: number
  durationMinutes: number
  parts: ToeicPart[]
  difficulty: Difficulty
  publishedAt?: string
}

export interface UserTestResult {
  id: string
  userId: string
  testId: string
  test?: Test
  listeningRaw: number
  readingRaw: number
  listeningScaled: number
  readingScaled: number
  totalScore: number
  durationSeconds: number
  answers: UserTestAnswer[]
  aiAnalysis?: AIAnalysis
  completedAt: string
}

export interface ExamResultDto extends UserTestResult {
  partBreakdown: PartScore[]
}

export interface ExamHistoryItem {
  id: string
  testId: string
  testTitle: string
  listeningScaled: number
  readingScaled: number
  totalScore: number
  completedAt: string
}

export interface PartScore {
  part: ToeicPart
  correct: number
  total: number
  percentage: number
}

export interface UserTestAnswer {
  questionId: string
  selectedIndex: number
  isCorrect: boolean
  timeSpentSeconds: number
}

// ─── AI Analysis ─────────────────────────────────────────────────────────────

export interface AIAnalysis {
  id: string
  resultId: string
  summary: string
  weakParts: WeakPoint[]
  studyPlan: WeeklyPlan[]
  dailyMinutes: number
  predictedScore: number
  successProbability: number
  generatedAt: string
}

export interface WeakPoint {
  part: ToeicPart
  accuracy: number
  priorityLevel: 'high' | 'medium' | 'low'
  recommendation: string
}

export interface WeeklyPlan {
  week: number
  focus: string
  tasks: string[]
}

// ─── Dictionary ──────────────────────────────────────────────────────────────

export interface DictionaryEntry {
  word: string
  phonetic: string
  audioUrl?: string
  meanings: DictionaryMeaning[]
  collocations: string[]
  toeicExamples: string[]
}

export interface DictionaryMeaning {
  partOfSpeech: string
  definitionVi: string
  definitionEn: string
  examples: string[]
}

// ─── Subscription ────────────────────────────────────────────────────────────

export interface Subscription {
  id: string
  userId: string
  plan: SubscriptionPlan
  status: 'active' | 'cancelled' | 'expired' | 'trial'
  startedAt: string
  expiresAt: string
  autoRenew: boolean
}

export interface Transaction {
  id: string
  userId: string
  subscriptionId?: string
  amount: number
  currency: 'VND' | 'USD'
  status: 'success' | 'pending' | 'failed' | 'refunded'
  provider: 'payos' | 'stripe'
  providerRef?: string
  createdAt: string
}

// ─── Ranking ─────────────────────────────────────────────────────────────────

export type RankingPeriod = 'weekly' | 'monthly' | 'all'
export type RankingType = 'exam' | 'xp'

export interface RankingEntry {
  rank: number
  userId: string
  fullName: string
  avatarUrl?: string
  level: number
  bestScore: number
  testsTaken: number
  xpTotal: number
}

export interface UserRankResult {
  rank: number
  entry: RankingEntry | null
}

// ─── Admin ───────────────────────────────────────────────────────────────────

export interface AdminStats {
  totalUsers: number
  newUsersToday: number
  activeUsersToday: number
  revenueToday: number
  revenueThisMonth: number
  testsCompletedToday: number
  subscriptionCount: Record<SubscriptionPlan, number>
}

// ─── API Responses ───────────────────────────────────────────────────────────

export interface ApiResponse<T> {
  data: T
  message?: string
  success: boolean
}

export interface PaginatedResponse<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export interface ApiError {
  message: string
  code?: string
  statusCode: number
}
