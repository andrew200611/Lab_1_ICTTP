-- Скрипт для оновлення Foreign Key constraints для каскадного видалення
-- Виконати цей скрипт безпосередньо в базі даних PostgreSQL (lab1db)

-- 1. Оновлення Movie_genres (MovieGenre junction table)
-- Видаляємо старий FK для Movie
ALTER TABLE "Movie_genres" 
DROP CONSTRAINT IF EXISTS "FK_Movie_Genres_Movie";

-- Створюємо новий FK з CASCADE для Movie
ALTER TABLE "Movie_genres" 
ADD CONSTRAINT "FK_Movie_Genres_Movie" 
FOREIGN KEY ("Mv_id") 
REFERENCES "Movies" ("Mv_id") 
ON DELETE CASCADE;

-- Оновлюємо FK для Genre (Restrict щоб не видалити жанр)
ALTER TABLE "Movie_genres" 
DROP CONSTRAINT IF EXISTS "FK_Movie_Genres_Genre";

ALTER TABLE "Movie_genres" 
ADD CONSTRAINT "FK_Movie_Genres_Genre" 
FOREIGN KEY ("Gr_id") 
REFERENCES "Genres" ("Gr_id") 
ON DELETE RESTRICT;

-- 2. Оновлення Movie_actors (MovieActor junction table)
-- Видаляємо старий FK для Movie
ALTER TABLE "Movie_actors" 
DROP CONSTRAINT IF EXISTS "FK_MA_Movie";

-- Створюємо новий FK з CASCADE для Movie
ALTER TABLE "Movie_actors" 
ADD CONSTRAINT "FK_MA_Movie" 
FOREIGN KEY ("Mv_id") 
REFERENCES "Movies" ("Mv_id") 
ON DELETE CASCADE;

-- Оновлюємо FK для Actor (Restrict щоб не видалити актора)
ALTER TABLE "Movie_actors" 
DROP CONSTRAINT IF EXISTS "FK_MA_Actor";

ALTER TABLE "Movie_actors" 
ADD CONSTRAINT "FK_MA_Actor" 
FOREIGN KEY ("Act_id") 
REFERENCES "Actors" ("Act_id") 
ON DELETE RESTRICT;

-- 3. Оновлення Favourites (User-Movie junction table)
-- Видаляємо старий FK для Movie
ALTER TABLE "Favourites" 
DROP CONSTRAINT IF EXISTS "FK_Fav_Movie";

-- Створюємо новий FK з CASCADE для Movie
ALTER TABLE "Favourites" 
ADD CONSTRAINT "FK_Fav_Movie" 
FOREIGN KEY ("Fav_movie") 
REFERENCES "Movies" ("Mv_id") 
ON DELETE CASCADE;

-- Оновлюємо FK для User з CASCADE
ALTER TABLE "Favourites" 
DROP CONSTRAINT IF EXISTS "FK_Fav_User";

ALTER TABLE "Favourites" 
ADD CONSTRAINT "FK_Fav_User" 
FOREIGN KEY ("Fav_User") 
REFERENCES "Users" ("Us_ID") 
ON DELETE CASCADE;

-- 4. Оновлення Reviews
-- Видаляємо старий FK для Movie
ALTER TABLE "Reviews" 
DROP CONSTRAINT IF EXISTS "FK_Review_Movie";

-- Створюємо новий FK з CASCADE для Movie
ALTER TABLE "Reviews" 
ADD CONSTRAINT "FK_Review_Movie" 
FOREIGN KEY ("RW_Movie") 
REFERENCES "Movies" ("Mv_id") 
ON DELETE CASCADE;

-- FK для User залишаємо ClientSetNull (NO ACTION), щоб відгуки не видалялися при видаленні користувача
-- Але якщо хочете CASCADE для User теж, розкоментуйте:
-- ALTER TABLE "Reviews" 
-- DROP CONSTRAINT IF EXISTS "FK_Review_User";
-- 
-- ALTER TABLE "Reviews" 
-- ADD CONSTRAINT "FK_Review_User" 
-- FOREIGN KEY ("RW_User") 
-- REFERENCES "Users" ("Us_ID") 
-- ON DELETE CASCADE;

-- Перевірка результатів
SELECT 
    tc.constraint_name,
    tc.table_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name,
    rc.delete_rule
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
LEFT JOIN information_schema.referential_constraints AS rc
    ON rc.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
    AND tc.table_name IN ('Movie_genres', 'Movie_actors', 'Favourites', 'Reviews')
ORDER BY tc.table_name, tc.constraint_name;
