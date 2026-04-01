-- Sample vocabulary seed data (10 words to get started)

INSERT INTO vocabulary (word, phonetic, meaning_vi, meaning_en, part_of_speech, examples, collocations, topic, level)
VALUES
('consider', '/kənˈsɪdər/', 'xem xét, cân nhắc', 'to think carefully about something', 'verb',
 '["We should consider all available options.", "Please consider my proposal carefully."]',
 '["consider + V-ing", "take into consideration", "consider carefully"]',
 'Business', 'intermediate'),

('implement', '/ˈɪmplɪment/', 'thực hiện, triển khai', 'to put a plan or system into action', 'verb',
 '["The company plans to implement new policies.", "We need to implement this strategy quickly."]',
 '["implement a plan", "fully implement", "implement changes"]',
 'Business', 'intermediate'),

('collaborate', '/kəˈlæbəreɪt/', 'hợp tác, cộng tác', 'to work jointly with others on a project', 'verb',
 '["We collaborate with international partners.", "Teams collaborate on complex projects."]',
 '["collaborate with", "collaborate on", "closely collaborate"]',
 'Business', 'advanced'),

('revenue', '/ˈrevənjuː/', 'doanh thu, thu nhập', 'income generated from business operations', 'noun',
 '["Annual revenue increased by 15%.", "Revenue from online sales is growing."]',
 '["revenue growth", "annual revenue", "revenue stream", "generate revenue"]',
 'Finance', 'intermediate'),

('deadline', '/ˈdedlaɪn/', 'hạn chót, thời hạn', 'the latest time or date by which something must be done', 'noun',
 '["The deadline for submissions is Friday.", "We must meet the project deadline."]',
 '["meet a deadline", "miss a deadline", "tight deadline", "set a deadline"]',
 'Office', 'basic'),

('efficient', '/ɪˈfɪʃənt/', 'hiệu quả, hiệu suất', 'achieving maximum productivity with minimum waste', 'adjective',
 '["We need a more efficient system.", "She is very efficient at her job."]',
 '["highly efficient", "cost-efficient", "efficient use of", "efficient process"]',
 'Business', 'basic'),

('accommodate', '/əˈkɒmədeɪt/', 'đáp ứng, điều chỉnh cho phù hợp', 'to fit in with the wishes of someone', 'verb',
 '["We can accommodate your request.", "The hotel accommodates 500 guests."]',
 '["accommodate requests", "accommodate guests", "accommodate changes"]',
 'Travel', 'intermediate'),

('negotiate', '/nɪˈɡoʊʃieɪt/', 'đàm phán, thương lượng', 'to discuss to reach an agreement', 'verb',
 '["We need to negotiate a better price.", "Both sides agreed to negotiate."]',
 '["negotiate a deal", "negotiate terms", "negotiate with", "successfully negotiate"]',
 'Business', 'intermediate'),

('quarterly', '/ˈkwɔːrtərli/', 'hàng quý', 'happening or produced four times a year', 'adjective',
 '["The quarterly report shows strong growth.", "We hold quarterly reviews."]',
 '["quarterly report", "quarterly earnings", "quarterly meeting", "quarterly basis"]',
 'Finance', 'basic'),

('resign', '/rɪˈzaɪn/', 'từ chức, nghỉ việc', 'to officially leave a job', 'verb',
 '["She decided to resign from her position.", "He resigned after the scandal."]',
 '["resign from", "formally resign", "resign a position", "letter of resignation"]',
 'Human Resources', 'basic');
