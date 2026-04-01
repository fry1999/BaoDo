-- BaoDo — Initial Schema Migration
-- Run this against your Supabase PostgreSQL database

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ─── Enums ────────────────────────────────────────────────────────────────────

CREATE TYPE user_role AS ENUM ('user', 'content_editor', 'admin');
CREATE TYPE subscription_plan AS ENUM ('free', 'basic', 'pro');
CREATE TYPE srs_status AS ENUM ('new', 'learning', 'review', 'mastered');
CREATE TYPE part_of_speech AS ENUM ('noun', 'verb', 'adjective', 'adverb', 'preposition', 'conjunction', 'phrase');
CREATE TYPE vocab_topic AS ENUM ('Business', 'Finance', 'Office', 'Travel', 'Technology', 'HumanResources', 'Marketing', 'Legal', 'General');
CREATE TYPE vocab_level AS ENUM ('basic', 'intermediate', 'advanced');
CREATE TYPE grammar_category AS ENUM ('tenses', 'word_form', 'prepositions', 'conjunctions', 'articles', 'subject_verb_agreement', 'relative_clauses', 'conditionals', 'passive_voice', 'comparison');
CREATE TYPE toeic_part AS ENUM ('1', '2', '3', '4', '5', '6', '7');
CREATE TYPE difficulty AS ENUM ('easy', 'medium', 'hard');
CREATE TYPE test_type AS ENUM ('full', 'mini', 'part');
CREATE TYPE passage_type AS ENUM ('single', 'double', 'triple');
CREATE TYPE dictation_source AS ENUM ('library', 'youtube');
CREATE TYPE dictation_level AS ENUM ('beginner', 'intermediate', 'advanced');
CREATE TYPE dictation_topic AS ENUM ('Business', 'Conversation', 'Announcement', 'News', 'Lecture');
CREATE TYPE subscription_status AS ENUM ('active', 'cancelled', 'expired', 'trial');
CREATE TYPE transaction_status AS ENUM ('success', 'pending', 'failed', 'refunded');
CREATE TYPE payment_provider AS ENUM ('payos', 'stripe');

-- ─── Profiles ─────────────────────────────────────────────────────────────────

CREATE TABLE profiles (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email           VARCHAR(255) NOT NULL UNIQUE,
    full_name       VARCHAR(255) NOT NULL,
    avatar_url      TEXT,
    role            user_role NOT NULL DEFAULT 'user',
    subscription    subscription_plan NOT NULL DEFAULT 'free',
    target_score    INT NOT NULL DEFAULT 700,
    exam_date       DATE,
    streak_count    INT NOT NULL DEFAULT 0,
    xp_total        INT NOT NULL DEFAULT 0,
    level           INT NOT NULL DEFAULT 1,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_active_at  TIMESTAMPTZ
);

-- ─── Vocabulary ───────────────────────────────────────────────────────────────

CREATE TABLE vocabulary (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    word            VARCHAR(200) NOT NULL,
    phonetic        VARCHAR(200),
    audio_url       TEXT,
    meaning_vi      TEXT NOT NULL,
    meaning_en      TEXT NOT NULL,
    part_of_speech  part_of_speech NOT NULL,
    examples        JSONB NOT NULL DEFAULT '[]',
    collocations    JSONB NOT NULL DEFAULT '[]',
    topic           vocab_topic NOT NULL,
    level           vocab_level NOT NULL,
    image_url       TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_vocabulary_word ON vocabulary(word);
CREATE INDEX idx_vocabulary_topic ON vocabulary(topic);
CREATE INDEX idx_vocabulary_level ON vocabulary(level);

CREATE TABLE user_vocabulary_cards (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id         UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
    vocab_id        UUID NOT NULL REFERENCES vocabulary(id) ON DELETE CASCADE,
    status          srs_status NOT NULL DEFAULT 'new',
    interval        INT NOT NULL DEFAULT 1,
    ease_factor     DECIMAL(4,2) NOT NULL DEFAULT 2.5,
    repetitions     INT NOT NULL DEFAULT 0,
    next_review_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_reviewed_at TIMESTAMPTZ,
    UNIQUE(user_id, vocab_id)
);

CREATE INDEX idx_user_vocab_cards_due ON user_vocabulary_cards(user_id, next_review_at);

-- ─── Grammar ──────────────────────────────────────────────────────────────────

CREATE TABLE grammar_lessons (
    id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title               VARCHAR(255) NOT NULL,
    category            grammar_category NOT NULL,
    content             TEXT NOT NULL,
    difficulty          SMALLINT NOT NULL DEFAULT 1 CHECK (difficulty BETWEEN 1 AND 3),
    estimated_minutes   INT NOT NULL DEFAULT 10,
    is_published        BOOLEAN NOT NULL DEFAULT false,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE grammar_questions (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    lesson_id       UUID NOT NULL REFERENCES grammar_lessons(id) ON DELETE CASCADE,
    question_text   TEXT NOT NULL,
    options         JSONB NOT NULL,
    correct_index   SMALLINT NOT NULL,
    explanation     TEXT NOT NULL,
    sort_order      INT NOT NULL DEFAULT 0
);

-- ─── Questions ────────────────────────────────────────────────────────────────

CREATE TABLE passages (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    part            SMALLINT NOT NULL,
    passage_type    passage_type NOT NULL DEFAULT 'single',
    title           VARCHAR(255),
    content         TEXT NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE questions (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    part            SMALLINT NOT NULL CHECK (part BETWEEN 1 AND 7),
    difficulty      difficulty NOT NULL DEFAULT 'medium',
    tags            JSONB NOT NULL DEFAULT '[]',
    question_text   TEXT,
    image_url       TEXT,
    audio_url       TEXT,
    transcript      TEXT,
    options         JSONB NOT NULL,
    correct_index   SMALLINT NOT NULL,
    explanation     TEXT NOT NULL,
    passage_id      UUID REFERENCES passages(id) ON DELETE SET NULL,
    is_published    BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_questions_part ON questions(part);
CREATE INDEX idx_questions_published ON questions(is_published, part);

-- ─── Tests ────────────────────────────────────────────────────────────────────

CREATE TABLE tests (
    id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title               VARCHAR(255) NOT NULL,
    type                test_type NOT NULL DEFAULT 'full',
    total_questions     INT NOT NULL DEFAULT 200,
    duration_minutes    INT NOT NULL DEFAULT 120,
    difficulty          difficulty NOT NULL DEFAULT 'medium',
    is_published        BOOLEAN NOT NULL DEFAULT false,
    published_at        TIMESTAMPTZ,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE test_questions (
    test_id         UUID NOT NULL REFERENCES tests(id) ON DELETE CASCADE,
    question_id     UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    sort_order      INT NOT NULL DEFAULT 0,
    PRIMARY KEY (test_id, question_id)
);

CREATE TABLE user_test_results (
    id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id             UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
    test_id             UUID NOT NULL REFERENCES tests(id),
    listening_raw       INT NOT NULL DEFAULT 0,
    reading_raw         INT NOT NULL DEFAULT 0,
    listening_scaled    INT NOT NULL DEFAULT 0,
    reading_scaled      INT NOT NULL DEFAULT 0,
    total_score         INT NOT NULL DEFAULT 0,
    duration_seconds    INT NOT NULL DEFAULT 0,
    completed_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_test_results_user ON user_test_results(user_id, completed_at DESC);

CREATE TABLE user_test_answers (
    id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    result_id           UUID NOT NULL REFERENCES user_test_results(id) ON DELETE CASCADE,
    question_id         UUID NOT NULL REFERENCES questions(id),
    selected_index      SMALLINT NOT NULL DEFAULT -1,
    is_correct          BOOLEAN NOT NULL DEFAULT false,
    time_spent_seconds  INT NOT NULL DEFAULT 0
);

CREATE TABLE ai_analyses (
    id                      UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    result_id               UUID NOT NULL UNIQUE REFERENCES user_test_results(id) ON DELETE CASCADE,
    summary                 TEXT NOT NULL DEFAULT '',
    weak_parts              JSONB NOT NULL DEFAULT '[]',
    study_plan              JSONB NOT NULL DEFAULT '[]',
    daily_minutes           INT NOT NULL DEFAULT 45,
    predicted_score         INT NOT NULL DEFAULT 0,
    success_probability     INT NOT NULL DEFAULT 50,
    generated_at            TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ─── Dictation ────────────────────────────────────────────────────────────────

CREATE TABLE dictation_content (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title           VARCHAR(255) NOT NULL,
    source          dictation_source NOT NULL DEFAULT 'library',
    youtube_id      VARCHAR(50),
    audio_url       TEXT,
    level           dictation_level NOT NULL DEFAULT 'intermediate',
    topic           dictation_topic NOT NULL DEFAULT 'Business',
    duration        INT NOT NULL DEFAULT 0,
    is_published    BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE dictation_segments (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    content_id      UUID NOT NULL REFERENCES dictation_content(id) ON DELETE CASCADE,
    index           INT NOT NULL,
    start           DECIMAL(8,3) NOT NULL,
    "end"           DECIMAL(8,3) NOT NULL,
    text            TEXT NOT NULL,
    UNIQUE(content_id, index)
);

CREATE TABLE user_dictation_progress (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id         UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
    content_id      UUID NOT NULL REFERENCES dictation_content(id) ON DELETE CASCADE,
    segment_index   INT NOT NULL,
    user_input      TEXT NOT NULL DEFAULT '',
    accuracy        SMALLINT NOT NULL DEFAULT 0,
    attempts        INT NOT NULL DEFAULT 0,
    completed_at    TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(user_id, content_id, segment_index)
);

-- ─── Subscriptions ────────────────────────────────────────────────────────────

CREATE TABLE subscriptions (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id         UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
    plan            subscription_plan NOT NULL,
    status          subscription_status NOT NULL DEFAULT 'active',
    started_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at      TIMESTAMPTZ NOT NULL,
    auto_renew      BOOLEAN NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE transactions (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id         UUID NOT NULL REFERENCES profiles(id),
    subscription_id UUID REFERENCES subscriptions(id),
    amount          DECIMAL(12,2) NOT NULL,
    currency        VARCHAR(3) NOT NULL DEFAULT 'VND',
    status          transaction_status NOT NULL DEFAULT 'pending',
    provider        payment_provider NOT NULL,
    provider_ref    VARCHAR(255),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_transactions_user ON transactions(user_id, created_at DESC);
