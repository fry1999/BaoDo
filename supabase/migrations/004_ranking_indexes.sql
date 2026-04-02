-- BaoDo — Migration 004: Ranking performance indexes + password hash column

-- Add password_hash column to profiles (for BCrypt-hashed passwords)
ALTER TABLE profiles ADD COLUMN IF NOT EXISTS password_hash TEXT;

-- Index for exam leaderboard queries (best score per user, filtered by period)
CREATE INDEX IF NOT EXISTS idx_test_results_score
    ON user_test_results(total_score DESC, completed_at DESC);

-- Index for XP leaderboard queries
CREATE INDEX IF NOT EXISTS idx_profiles_xp
    ON profiles(xp_total DESC);

-- Composite index to efficiently compute per-user best scores
CREATE INDEX IF NOT EXISTS idx_test_results_user_score
    ON user_test_results(user_id, total_score DESC);
