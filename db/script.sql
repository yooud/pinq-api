DROP TYPE IF EXISTS location_visibility;
DROP TYPE IF EXISTS friend_request_status;
DROP TYPE IF EXISTS event_visibility;
DROP TYPE IF EXISTS event_participant_status;
DROP TYPE IF EXISTS complaint_content_type;
DROP TYPE IF EXISTS complaint_status;
DROP TYPE IF EXISTS moderation_complaint_status;
DROP TYPE IF EXISTS photo_type;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS user_profiles;
DROP TABLE IF EXISTS user_sessions;
DROP TABLE IF EXISTS locations;
DROP TABLE IF EXISTS location_settings;
DROP TABLE IF EXISTS user_markers;
DROP TABLE IF EXISTS markers;
DROP TABLE IF EXISTS friends;
DROP TABLE IF EXISTS friends_requests;
DROP TABLE IF EXISTS blocked_users;
DROP TABLE IF EXISTS chats;
DROP TABLE IF EXISTS chat_messages;
DROP TABLE IF EXISTS events;
DROP TABLE IF EXISTS event_participants;
DROP TABLE IF EXISTS post_views;
DROP TABLE IF EXISTS complaints;
DROP TABLE IF EXISTS moderation_complaints;
DROP TABLE IF EXISTS photos;
DROP TABLE IF EXISTS posts;
DROP TABLE IF EXISTS posts;

CREATE TYPE location_visibility AS ENUM('hidden', 'approximate', 'normal');

CREATE TYPE friend_request_status AS ENUM('pending', 'accepted', 'rejected', 'canceled');

CREATE TYPE event_visibility AS ENUM('public','friends','private');

CREATE TYPE event_participant_status AS ENUM('invited','joined','rejected');

CREATE TYPE complaint_content_type AS ENUM('post', 'avatar', 'user');

CREATE TYPE complaint_status AS ENUM('pending','in_review','approved','rejected');

CREATE TYPE moderation_complaint_status AS ENUM('deleted','remained');

CREATE TYPE photo_type AS ENUM('avatar','post','chat');

CREATE TYPE event_status AS ENUM('moderation', 'accepted', 'rejected', 'canceled');

CREATE TYPE moderation_event_status AS ENUM('accepted','rejected');

CREATE TABLE "users" (
                         "id" serial PRIMARY KEY,
                         "uid" varchar UNIQUE NOT NULL,
                         "email" varchar(100) UNIQUE NOT NULL,
                         "is_banned" bool NOT NULL DEFAULT false,
                         "banned_at" timestamp,
                         "created_at" timestamp DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE "user_profiles" (
                                 "user_id" int PRIMARY KEY,
                                 "username" varchar(20) UNIQUE NOT NULL,
                                 "display_name" varchar(50) NOT NULL,
                                 "status" varchar(25),
                                 "photo_id" int,
                                 "battery_status" smallint
);

CREATE TABLE "user_sessions" (
                                 "uid" varchar PRIMARY KEY,
                                 "session_token" uuid UNIQUE NOT NULL,
                                 "fcm_token" text NOT NULL,
                                 "last_login" timestamp NOT NULL
);

CREATE TABLE "locations" (
                             "id" serial,
                             "user_id" int,
                             "geom" GEOMETRY(Point,4326) NOT NULL,
                             "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                             PRIMARY KEY ("id", "user_id")
);

CREATE TABLE "location_settings" (
                                     "user_id" int PRIMARY KEY,
                                     "global_visibility" location_visibility NOT NULL
);

CREATE TABLE "friend_location_visibility" (
                                              "user_id" int,
                                              "friend_id" int,
                                              "visibility" location_visibility NOT NULL,
                                              PRIMARY KEY ("user_id", "friend_id")
);

CREATE TABLE "user_markers" (
                                "user_id" int,
                                "marker_id" int,
                                "geom" GEOMETRY(Point,4326) NOT NULL,
                                PRIMARY KEY ("user_id", "marker_id")
);

CREATE TABLE "markers" (
                           "id" serial PRIMARY KEY,
                           "name" varchar NOT NULL
);

CREATE TABLE "friends" (
                           "user_id" int,
                           "friend_id" int,
                           "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                           PRIMARY KEY ("user_id", "friend_id")
);

CREATE TABLE "friends_requests" (
                                    "id" serial PRIMARY KEY,
                                    "sender_id" int,
                                    "receiver_id" int,
                                    "status" friend_request_status NOT NULL,
                                    "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                                    "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE "blocked_users" (
                                 "user_id" int,
                                 "blocked_user_id" int,
                                 "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                                 PRIMARY KEY ("user_id", "blocked_user_id")
);

CREATE TABLE "chats" (
                         "id" serial PRIMARY KEY,
                         "user_id1" int,
                         "user_id2" int,
                         "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE "chat_messages" (
                                 "id" serial PRIMARY KEY,
                                 "chat_id" int,
                                 "sender_id" int,
                                 "content" json NOT NULL,
                                 "sent_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                                 "edited_at" timestamp,
                                 "seen_at" timestamp,
                                 "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE "events" (
                          "id" serial PRIMARY KEY,
                          "creator_id" int,
                          "name" varchar NOT NULL,
                          "description" text NOT NULL,
                          "geom" GEOMETRY(Point,4326) NOT NULL,
                          "start_time" timestamp NOT NULL,
                          "end_time" timestamp NOT NULL,
                          "visibility" event_visibility NOT NULL,
                          "status" event_status NOT NULL,
                          "is_active" bool NOT NULL DEFAULT (false),
                          "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE "event_participants" (
                                      "event_id" int,
                                      "user_id" int,
                                      "status" event_participant_status NOT NULL,
                                      PRIMARY KEY ("event_id", "user_id")
);

CREATE TABLE "posts" (
                         "id" serial PRIMARY KEY,
                         "user_id" int,
                         "photo_id" int,
                         "geom" GEOMETRY(Point,4326) NOT NULL,
                         "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE "post_views" (
                              "post_id" int,
                              "viewer_id" int,
                              "viewed_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                              PRIMARY KEY ("post_id", "viewer_id")
);

CREATE TABLE "complaints" (
                              "id" serial PRIMARY KEY,
                              "user_id" int,
                              "target_user_id" int,
                              "content_type" complaint_content_type NOT NULL,
                              "content_id" int,
                              "reason" text NOT NULL,
                              "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                              "status" complaint_status NOT NULL
);

CREATE TABLE "moderation_complaints" (
                                         "id" serial PRIMARY KEY,
                                         "moderator_id" int,
                                         "complaint_id" int,
                                         "decision" moderation_complaint_status NOT NULL,
                                         "decision_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE "photos" (
                          "id" serial PRIMARY KEY,
                          "user_id" int,
                          "photo_type" photo_type NOT NULL,
                          "image_code" text NOT NULL,
                          "image_url" text NOT NULL,
                          "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE "moderation_events" (
                                     "id" serial PRIMARY KEY,
                                     "moderator_id" int,
                                     "event_id" int,
                                     "decision" moderation_event_status NOT NULL,
                                     "decision_content" text,
                                     "decision_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

CREATE INDEX ON "users" ("is_banned", "banned_at");

CREATE INDEX ON "user_profiles" ("photo_id");

CREATE INDEX ON "user_profiles" ("user_id");

CREATE INDEX ON "user_sessions" ("user_id");

CREATE INDEX ON "user_sessions" ("session_token");

CREATE INDEX ON "user_sessions" ("last_login");

CREATE INDEX ON "locations" ("user_id");

CREATE INDEX ON "locations" ("created_at");

CREATE INDEX ON "locations" USING GIST("geom");

CREATE INDEX ON "location_settings" ("user_id");

CREATE INDEX ON "friend_location_visibility" ("friend_id");

CREATE INDEX ON "user_markers" USING GIST("geom");

CREATE INDEX ON "friends" ("friend_id");

CREATE INDEX ON "friends_requests" ("sender_id");

CREATE INDEX ON "friends_requests" ("receiver_id");

CREATE INDEX ON "friends_requests" ("sender_id", "status");

CREATE INDEX ON "friends_requests" ("receiver_id", "status");

CREATE UNIQUE INDEX ON "chats" ("user_id1", "user_id2");

CREATE INDEX ON "chats" ("user_id1");

CREATE INDEX ON "chats" ("user_id2");

CREATE INDEX ON "chat_messages" ("chat_id");

CREATE INDEX ON "chat_messages" ("sent_at");

CREATE INDEX ON "events" ("creator_id");

CREATE INDEX ON "events" ("start_time");

CREATE INDEX ON "events" ("visibility");

CREATE INDEX ON "events" USING GIST("geom");

CREATE INDEX ON "events" ("status");

CREATE INDEX ON "events" ("is_active");

CREATE INDEX ON "event_participants" ("user_id");

CREATE INDEX ON "event_participants" ("status");

CREATE INDEX ON "posts" ("user_id");

CREATE INDEX ON "posts" ("created_at");

CREATE INDEX ON "posts" USING GIST("geom");

CREATE INDEX ON "post_views" ("viewed_at");

CREATE INDEX ON "complaints" ("user_id");

CREATE INDEX ON "complaints" ("target_user_id");

CREATE INDEX ON "complaints" ("content_type");

CREATE INDEX ON "complaints" ("created_at");

CREATE INDEX ON "complaints" ("status");

CREATE INDEX ON moderation_complaints ("moderator_id");

CREATE INDEX ON moderation_complaints ("complaint_id");

CREATE INDEX ON moderation_complaints ("decision");

CREATE INDEX ON moderation_complaints ("decision_at");

CREATE INDEX ON moderation_events ("moderator_id");

CREATE INDEX ON moderation_events ("event_id");

CREATE INDEX ON moderation_events ("decision");

CREATE INDEX ON moderation_events ("decision_at");

ALTER TABLE "user_profiles" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "user_profiles" ADD FOREIGN KEY ("photo_id") REFERENCES "photos" ("id") ON DELETE SET NULL;

ALTER TABLE "user_sessions" ADD FOREIGN KEY ("uid") REFERENCES "users" ("uid") ON DELETE CASCADE;

ALTER TABLE "locations" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "location_settings" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "friend_location_visibility" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "friend_location_visibility" ADD FOREIGN KEY ("friend_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "user_markers" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "user_markers" ADD FOREIGN KEY ("marker_id") REFERENCES "markers" ("id") ON DELETE CASCADE;

ALTER TABLE "friends" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "friends" ADD FOREIGN KEY ("friend_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "friends_requests" ADD FOREIGN KEY ("sender_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "friends_requests" ADD FOREIGN KEY ("receiver_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "blocked_users" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "blocked_users" ADD FOREIGN KEY ("blocked_user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "chats" ADD FOREIGN KEY ("user_id1") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "chats" ADD FOREIGN KEY ("user_id2") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "chat_messages" ADD FOREIGN KEY ("chat_id") REFERENCES "chats" ("id") ON DELETE CASCADE;

ALTER TABLE "chat_messages" ADD FOREIGN KEY ("sender_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "events" ADD FOREIGN KEY ("creator_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "event_participants" ADD FOREIGN KEY ("event_id") REFERENCES "events" ("id") ON DELETE CASCADE;

ALTER TABLE "event_participants" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "posts" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "posts" ADD FOREIGN KEY ("photo_id") REFERENCES "photos" ("id") ON DELETE CASCADE;

ALTER TABLE "post_views" ADD FOREIGN KEY ("post_id") REFERENCES "posts" ("id") ON DELETE CASCADE;

ALTER TABLE "post_views" ADD FOREIGN KEY ("viewer_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "complaints" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE SET NULL;

ALTER TABLE "complaints" ADD FOREIGN KEY ("target_user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "complaints" ADD FOREIGN KEY ("content_id") REFERENCES "posts" ("id") ON DELETE CASCADE;

ALTER TABLE moderation_complaints ADD FOREIGN KEY ("moderator_id") REFERENCES "users" ("id") ON DELETE SET NULL;

ALTER TABLE moderation_complaints ADD FOREIGN KEY ("complaint_id") REFERENCES "complaints" ("id") ON DELETE CASCADE;

ALTER TABLE "photos" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE moderation_events ADD FOREIGN KEY ("moderator_id") REFERENCES "users" ("id") ON DELETE SET NULL;

ALTER TABLE moderation_events ADD FOREIGN KEY ("event_id") REFERENCES "events" ("id") ON DELETE CASCADE;

INSERT INTO markers (name) VALUES ('home'), ('work'), ('education');