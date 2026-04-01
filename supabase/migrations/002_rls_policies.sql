-- Row Level Security Policies for Supabase

-- Enable RLS on user-specific tables
ALTER TABLE profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_vocabulary_cards ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_test_results ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_test_answers ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_dictation_progress ENABLE ROW LEVEL SECURITY;
ALTER TABLE subscriptions ENABLE ROW LEVEL SECURITY;
ALTER TABLE transactions ENABLE ROW LEVEL SECURITY;

-- Profiles: users can read/update their own profile
CREATE POLICY "Users can view own profile"
    ON profiles FOR SELECT USING (auth.uid() = id);

CREATE POLICY "Users can update own profile"
    ON profiles FOR UPDATE USING (auth.uid() = id);

-- Vocabulary cards: users own their cards
CREATE POLICY "Users can manage own vocabulary cards"
    ON user_vocabulary_cards FOR ALL USING (auth.uid() = user_id);

-- Test results: users own their results
CREATE POLICY "Users can view own test results"
    ON user_test_results FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can insert own test results"
    ON user_test_results FOR INSERT WITH CHECK (auth.uid() = user_id);

-- Dictation progress: users own their progress
CREATE POLICY "Users can manage own dictation progress"
    ON user_dictation_progress FOR ALL USING (auth.uid() = user_id);

-- Public content: everyone can read published content
CREATE POLICY "Published vocabulary is public"
    ON vocabulary FOR SELECT USING (true);

CREATE POLICY "Published grammar lessons are public"
    ON grammar_lessons FOR SELECT USING (is_published = true);

CREATE POLICY "Published questions are public"
    ON questions FOR SELECT USING (is_published = true);

CREATE POLICY "Published tests are public"
    ON tests FOR SELECT USING (is_published = true);

CREATE POLICY "Published dictation is public"
    ON dictation_content FOR SELECT USING (is_published = true);
