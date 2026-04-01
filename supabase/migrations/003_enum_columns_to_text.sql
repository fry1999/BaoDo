-- Migration 003: Convert custom PostgreSQL enum columns to TEXT
-- Allows EF Core to store C# enum names as strings without type conflicts.
-- Run in Supabase SQL Editor.

-- profiles
ALTER TABLE profiles            ALTER COLUMN role           TYPE TEXT USING role::TEXT;
ALTER TABLE profiles            ALTER COLUMN subscription   TYPE TEXT USING subscription::TEXT;

-- vocabulary
ALTER TABLE vocabulary          ALTER COLUMN part_of_speech TYPE TEXT USING part_of_speech::TEXT;
ALTER TABLE vocabulary          ALTER COLUMN topic          TYPE TEXT USING topic::TEXT;
ALTER TABLE vocabulary          ALTER COLUMN level          TYPE TEXT USING level::TEXT;

-- user_vocabulary_cards
ALTER TABLE user_vocabulary_cards ALTER COLUMN status       TYPE TEXT USING status::TEXT;

-- grammar_lessons  (difficulty is SMALLINT — leave it, only category is enum)
ALTER TABLE grammar_lessons     ALTER COLUMN category       TYPE TEXT USING category::TEXT;

-- questions  (part is SMALLINT — leave it, only difficulty is enum)
ALTER TABLE questions           ALTER COLUMN difficulty     TYPE TEXT USING difficulty::TEXT;

-- passages
ALTER TABLE passages            ALTER COLUMN passage_type   TYPE TEXT USING passage_type::TEXT;

-- tests  (column name is "type", NOT "test_type")
ALTER TABLE tests               ALTER COLUMN type           TYPE TEXT USING type::TEXT;
ALTER TABLE tests               ALTER COLUMN difficulty     TYPE TEXT USING difficulty::TEXT;

-- dictation_content
ALTER TABLE dictation_content   ALTER COLUMN source         TYPE TEXT USING source::TEXT;
ALTER TABLE dictation_content   ALTER COLUMN level          TYPE TEXT USING level::TEXT;
ALTER TABLE dictation_content   ALTER COLUMN topic          TYPE TEXT USING topic::TEXT;

-- subscriptions
ALTER TABLE subscriptions       ALTER COLUMN plan           TYPE TEXT USING plan::TEXT;
ALTER TABLE subscriptions       ALTER COLUMN status         TYPE TEXT USING status::TEXT;

-- transactions
ALTER TABLE transactions        ALTER COLUMN status         TYPE TEXT USING status::TEXT;
ALTER TABLE transactions        ALTER COLUMN provider       TYPE TEXT USING provider::TEXT;
