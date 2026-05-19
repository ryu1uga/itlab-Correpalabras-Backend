SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

CREATE TABLE public."Attachment" (
    "Id" uuid NOT NULL,
    "StoryId" uuid NOT NULL,
    "LanguageId" uuid NOT NULL,
    "ImageUrl" character varying (255) NOT NULL,
    "TypeImage" character varying (255) NOT NULL,
    "Position" character varying (255) NOT NULL,
    "OrderAttachments" integer NOT NULL
);

ALTER TABLE public."Attachment" OWNER TO correpalabras;

CREATE TABLE public."Avatar" (
    "Id" uuid NOT NULL,
    "StoryId" uuid NOT NULL,
    "AvatarUrl" character varying (255) NOT NULL
);

ALTER TABLE public."Avatar" OWNER TO correpalabras;


CREATE TABLE public."Badge" (
    "Id" uuid NOT NULL,
    "Name" character varying (255) NOT NULL,
    "BadgeUrl" character varying (255) NOT NULL
);

ALTER TABLE public."Badge" OWNER TO correpalabras;


CREATE TABLE public."Category" (
    "Id" uuid NOT NULL,
    "Name" character varying (255) NOT NULL,
    "Code" character varying (255) NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CategoryOrder" integer NOT NULL,
    "Counter" integer NOT NULL
);

ALTER TABLE public."Category" OWNER TO correpalabras;


CREATE TABLE public."Language" (
    "Id" uuid NOT NULL,
    "Name" character varying (255) NOT NULL,
    "Counter" integer NOT NULL
);

ALTER TABLE public."Language" OWNER TO correpalabras;


CREATE TABLE public."Page" (
    "Id" uuid NOT NULL,
    "StoryId" uuid NOT NULL,
    "PageOrder" integer NOT NULL,
    "ImageUrl" character varying (255) NOT NULL
);

ALTER TABLE public."Page" OWNER TO correpalabras;


CREATE TABLE public."PageContent" (
    "Id" uuid NOT NULL,
    "PageId" uuid NOT NULL,
    "LanguageId" uuid NOT NULL,
    "CountWords" integer NOT NULL,
    "Content" text NOT NULL
);

ALTER TABLE public."PageContent" OWNER TO correpalabras;


CREATE TABLE public."Profile" (
    "Id" uuid NOT NULL,
    "AvatarId" uuid NOT NULL,
    "Username" text NOT NULL,
    "Gender" text NOT NULL,
    "BirthDate" timestamp with time zone NOT NULL,
    "UserId" uuid NOT NULL
);

ALTER TABLE public."Profile" OWNER TO correpalabras;


CREATE TABLE public."ProfileStory" (
    "Id" uuid NOT NULL,
    "StoryLanguageId" uuid NOT NULL,
    "ProfileId" uuid NOT NULL,
    "IsDownloaded" boolean NOT NULL,
    "IsRead" boolean NOT NULL,
    "StartTime" timestamp with time zone NOT NULL,
    "EndTime" timestamp with time zone NOT NULL
);

ALTER TABLE public."ProfileStory" OWNER TO correpalabras;


CREATE TABLE public."Story" (
    "Id" uuid NOT NULL,
    "Author" varchar NOT NULL,
    "Illustrator" varchar NOT NULL,
    "Title" varchar NOT NULL,
    "CountPages" integer NOT NULL,
    "Thumbnail" varchar NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "Counter" integer NOT NULL
);

ALTER TABLE public."Story" OWNER TO correpalabras;


CREATE TABLE public."StoryCategory" (
    "Id" uuid NOT NULL,
    "StoryId" uuid NOT NULL,
    "CategoryId" uuid NOT NULL
);

ALTER TABLE public."StoryCategory" OWNER TO correpalabras;


CREATE TABLE public."StoryLanguage" (
    "Id" uuid NOT NULL,
    "StoryId" uuid NOT NULL,
    "LanguageId" uuid NOT NULL
);

ALTER TABLE public."StoryLanguage" OWNER TO correpalabras;


CREATE TABLE public."UnlockedAvatar" (
    "Id" uuid NOT NULL,
    "ProfileId" uuid NOT NULL,
    "AvatarId" uuid NOT NULL
);

ALTER TABLE public."UnlockedAvatar" OWNER TO correpalabras;


CREATE TABLE public."UnlockedBadge" (
    "Id" uuid NOT NULL,
    "ProfileId" uuid NOT NULL,
    "BadgeId" uuid NOT NULL
);

ALTER TABLE public."UnlockedBadge" OWNER TO correpalabras;


CREATE TABLE public."User" (
    "Id" uuid NOT NULL,
    "Name" varchar NOT NULL,
    "Email" varchar NOT NULL,
    "Password" varchar NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "VerificationCode" integer,
    "CodeRegisteredDate" timestamp with time zone,
    "CodeExpirationDate" timestamp with time zone,
    "UserType" integer NOT NULL
);

ALTER TABLE public."User" OWNER TO correpalabras;


ALTER TABLE ONLY public."Attachment"
    ADD CONSTRAINT "Attachment_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."Avatar"
    ADD CONSTRAINT "Avatar_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."Badge"
    ADD CONSTRAINT "Badge_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."Category"
    ADD CONSTRAINT "Category_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."Language"
    ADD CONSTRAINT "Language_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."Page"
    ADD CONSTRAINT "Page_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."PageContent"
    ADD CONSTRAINT "PageContent_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."Profile"
    ADD CONSTRAINT "Profile_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."ProfileStory"
    ADD CONSTRAINT "ProfileStory_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."Story"
    ADD CONSTRAINT "Story_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."StoryCategory"
    ADD CONSTRAINT "StoryCategory_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."StoryLanguage"
    ADD CONSTRAINT "StoryLanguage_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."UnlockedAvatar"
    ADD CONSTRAINT "UnlockedAvatar_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."UnlockedBadge"
    ADD CONSTRAINT "UnlockedBadge_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."User"
    ADD CONSTRAINT "User_pkey" PRIMARY KEY ("Id");


ALTER TABLE ONLY public."Attachment"
    ADD CONSTRAINT "FK_ATTACHMENT_STORY" FOREIGN KEY ("StoryId") REFERENCES public."Story"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."Attachment"
    ADD CONSTRAINT "FK_ATTACHMENT_LANGUAGE" FOREIGN KEY ("LanguageId") REFERENCES public."Language"("Id");


ALTER TABLE ONLY public."Avatar"
    ADD CONSTRAINT "FK_AVATAR_STORY" FOREIGN KEY ("StoryId") REFERENCES public."Story"("Id") ON DELETE CASCADE;


ALTER TABLE ONLY public."Page"
    ADD CONSTRAINT "FK_Page_Story" FOREIGN KEY ("StoryId") REFERENCES public."Story" ("Id") ON DELETE CASCADE;


ALTER TABLE ONLY public."PageContent"
    ADD CONSTRAINT "FK_PageContent_Page" FOREIGN KEY ("PageId") REFERENCES public."Page" ("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."PageContent"
    ADD CONSTRAINT "FK_PageContent_Language" FOREIGN KEY ("LanguageId") REFERENCES public."Language" ("Id") ON DELETE CASCADE;


ALTER TABLE ONLY public."Profile"
    ADD CONSTRAINT "FK_Profile_Avatar" FOREIGN KEY ("AvatarId") REFERENCES public."Avatar" ("Id");

ALTER TABLE ONLY public."Profile"
    ADD CONSTRAINT "FK_Profile_User" FOREIGN KEY ("UserId") REFERENCES public."User" ("Id") ON DELETE CASCADE;


ALTER TABLE ONLY public."ProfileStory"
    ADD CONSTRAINT "FK_ProfileStory_StoryLanguage" FOREIGN KEY ("StoryLanguageId") REFERENCES public."StoryLanguage" ("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."ProfileStory"
    ADD CONSTRAINT "FK_ProfileStory_Profile" FOREIGN KEY ("ProfileId") REFERENCES public."Profile" ("Id") ON DELETE CASCADE;


ALTER TABLE ONLY public."StoryCategory"
    ADD CONSTRAINT "FK_Story_StoryCategory" FOREIGN KEY ("StoryId") REFERENCES public."Story" ("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."StoryCategory"
    ADD CONSTRAINT "FK_Category_StoryCategory" FOREIGN KEY ("CategoryId") REFERENCES public."Category" ("Id") ON DELETE CASCADE;


ALTER TABLE ONLY public."StoryLanguage"
    ADD CONSTRAINT "FK_Story_StoryLanguage" FOREIGN KEY ("StoryId") REFERENCES public."Story" ("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."StoryLanguage"
    ADD CONSTRAINT "FK_Language_StoryLanguage" FOREIGN KEY ("LanguageId") REFERENCES public."Language" ("Id") ON DELETE CASCADE;


ALTER TABLE ONLY public."UnlockedAvatar"
    ADD CONSTRAINT "FK_Profile_UnlockedAvatar" FOREIGN KEY ("ProfileId") REFERENCES public."Profile" ("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."UnlockedAvatar"
    ADD CONSTRAINT "FK_Avatar_UnlockedAvatar" FOREIGN KEY ("AvatarId") REFERENCES public."Avatar" ("Id") ON DELETE CASCADE;


ALTER TABLE ONLY public."UnlockedBadge"
    ADD CONSTRAINT "FK_Profile_UnlockedBadge" FOREIGN KEY ("ProfileId") REFERENCES public."Profile" ("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."UnlockedBadge"
    ADD CONSTRAINT "FK_Badge_UnlockedBadge" FOREIGN KEY ("BadgeId") REFERENCES public."Badge" ("Id") ON DELETE CASCADE;