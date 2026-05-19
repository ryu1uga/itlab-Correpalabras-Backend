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

-- Insertar datos en la tabla Language
INSERT INTO public."Language" ("Id", "Name", "Counter") VALUES
('5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 'Español', 0),
('d16f4bbb-ba33-4500-a5aa-371b450ea6f2', 'Quechua', 0);

-- Insertar datos en la tabla Story
INSERT INTO public."Story" ("Id", "Author", "Illustrator", "Title", "CountPages", "Thumbnail", "UpdatedAt", "Counter") VALUES
('8b29f0b0-ba85-400a-8e38-4996fdc82bbf', 'Jorge Montalvo', 'Alfredo Peña', '¿Dónde está mi moneda?', 8, 'http://correpalabrasprd.ulima.edu.pe/V2/story/thumbnails/THMONEDA.jpg', '2020-09-09 18:09:56.74', 0),
('975261fb-33f7-40b1-8afe-2390438ff977', 'Jorge Montalvo', 'Alfredo Peña', 'Apurados', 6, 'http://correpalabrasprd.ulima.edu.pe/V2/story/thumbnails/THAPURADOS.jpg', '2020-10-02 13:03:06.397', 0),
('c0f1d1cf-924b-46c3-95b1-d1657faec77c', 'Xiomayra Castillo', 'Xiomayra Castillo/Alfredo Peña', 'El árbol presumido', 5, 'http://correpalabrasprd.ulima.edu.pe/V2/story/thumbnails/THARBOLPR.jpg', '2021-05-31 11:15:49.2', 0),
('e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 'Jorge Montalvo', 'Sebastían Lino', 'El Arco Iris', 9, 'http://correpalabrasprd.ulima.edu.pe/V2/story/thumbnails/THARCOIRIS.jpg', '2019-09-17 14:37:13', 0);

-- Insertar datos en la tabla Attachment
INSERT INTO public."Attachment" ("Id", "StoryId", "LanguageId", "ImageUrl", "TypeImage", "Position", "OrderAttachments") VALUES
('2b566a13-5c30-48f7-94b3-067ea08a0d42', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/MONEDA/PORTMONEDA.jpg', 'Cover', '00', 0),
('86f7822b-f205-4a82-a874-366598e7eb9b', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/MONEDA/QHPORTMONEDA.jpg', 'Cover', '00', 0);
/*
INSERT INTO public."Attachment" ("Id", "StoryId", "LanguageId", "ImageUrl", "TypeImage", "Position", "OrderAttachments") VALUES
('aa63ae93-43d1-4f39-bd22-bf35d80361a9', '975261fb-33f7-40b1-8afe-2390438ff977', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/APURADOS/PORTAPURADOS.jpg', 'Cover', '00', 0),
('0efa70dd-57fb-49de-a7d5-d85e555f8d20', '975261fb-33f7-40b1-8afe-2390438ff977', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/APURADOS/QHPORTAPURADOS.jpg', 'Cover', '00', 0);

INSERT INTO public."Attachment" ("Id", "StoryId", "LanguageId", "ImageUrl", "TypeImage", "Position", "OrderAttachments") VALUES
('ed189367-7772-4e7c-8dce-69412e2a120d', 'c0f1d1cf-924b-46c3-95b1-d1657faec77c', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARBOL PRESUMIDO/PORTAARBOLPRES.jpg', 'Cover', '00', 0);

INSERT INTO public."Attachment" ("Id", "StoryId", "LanguageId", "ImageUrl", "TypeImage", "Position", "OrderAttachments") VALUES
('c65be315-509f-41ce-a952-6c0881546ca5', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARCOIRIS/1-ARCO-IRIS.jpg', 'Cover', '00', 0),
('bb062e4c-397d-44f7-ac6d-d1ec4b2d63d8', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARCOIRIS/1ARCO%20IRIS%20QUECHUA.jpg', 'Cover', '00', 0);
*/
-- Insertar datos en la tabla Page
INSERT INTO public."Page" ("Id", "StoryId", "PageOrder", "ImageUrl") VALUES
('6357df8a-e4c0-4026-adce-c6fb6b2fe693', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', 1, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/MONEDA/Moneda1.jpg'),
('a2048d27-feb0-4de1-ab6a-e93a80cd0cdb', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', 2, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/MONEDA/Moneda2.jpg'),
('3aee8bf5-7292-4ff7-9ce9-04eb6b043723', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', 3, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/MONEDA/Moneda3.jpg'),
('673e1721-0b1e-4d01-a46d-9413d5fe3d6c', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', 4, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/MONEDA/Moneda4.jpg'),
('ce729a7c-18ab-4dc6-a91c-24e68c9bf372', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', 5, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/MONEDA/Moneda5.jpg'),
('11b83be1-9b1e-47ea-8645-84e2cad14247', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', 6, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/MONEDA/Moneda6.jpg'),
('eeee1070-43e9-4d1e-99f7-f68dd024a18c', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', 7, 'http://correpalabrasprd.ulima.edu.pe/V2/story/vistafinal.jpg');
/*
INSERT INTO public."Page" ("Id", "StoryId", "PageOrder", "ImageUrl") VALUES
('b1e58e18-6bf8-42c3-91bb-53d265962dd5', '975261fb-33f7-40b1-8afe-2390438ff977', 1, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/APURADOS/1Apurados.jpg'),
('4e027afd-25f3-4c8b-a86c-8ed407c887e8', '975261fb-33f7-40b1-8afe-2390438ff977', 2, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/APURADOS/2Apurados.jpg'),
('ef08656d-fe03-46da-bcce-9b612bf533fc', '975261fb-33f7-40b1-8afe-2390438ff977', 3, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/APURADOS/3Apurados.jpg'),
('e9f6ebc5-c2b8-46db-867b-1a52e13a11ba', '975261fb-33f7-40b1-8afe-2390438ff977', 4, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/APURADOS/4Apurados.jpg'),
('d1c3f04e-67df-4e7e-9655-fc3cdf6551a8', '975261fb-33f7-40b1-8afe-2390438ff977', 5, 'http://correpalabrasprd.ulima.edu.pe/V2/story/vistafinal.jpg');

INSERT INTO public."Page" ("Id", "StoryId", "PageOrder", "ImageUrl") VALUES
('dff7998a-47b6-4553-b589-4df655b94b13', 'c0f1d1cf-924b-46c3-95b1-d1657faec77c', 1, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARBOL PRESUMIDO/1Arbolpr.jpg'),
('0b48cd67-5d13-43ce-90a2-a4a1e7e989d4', 'c0f1d1cf-924b-46c3-95b1-d1657faec77c', 2, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARBOL PRESUMIDO/2Arbolpr.jpg'),
('3e68b936-4075-4d50-ac05-fa9cfadaa34f', 'c0f1d1cf-924b-46c3-95b1-d1657faec77c', 3, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARBOL PRESUMIDO/3Arbolpr.jpg'),
('cba205d0-0b12-4d0c-8081-a1ac98b9180a', 'c0f1d1cf-924b-46c3-95b1-d1657faec77c', 4, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARBOL PRESUMIDO/4Arbolpr.jpg'),
('60f103ff-95fd-47c1-8d3a-599f3dde7564', 'c0f1d1cf-924b-46c3-95b1-d1657faec77c', 5, 'http://correpalabrasprd.ulima.edu.pe/V2/story/vistafinal.jpg');

INSERT INTO public."Page" ("Id", "StoryId", "PageOrder", "ImageUrl") VALUES
('a65097f9-b4d5-42c8-9323-d39f514721ef', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 1, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARCOIRIS/2-ARCO-IRIS.jpg'),
('27dd0b2f-09b3-4487-9d72-d991b566975c', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 2, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARCOIRIS/3-ARCO-IRIS.jpg'),
('c8b0eadb-f400-4896-9bdc-908110a5d658', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 3, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARCOIRIS/4-ARCO-IRIS.jpg'),
('0bac75a0-c35f-4223-bae3-03468437e5af', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 4, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARCOIRIS/5-ARCO-IRIS.jpg'),
('93432cd7-29cb-4424-9478-c8b1c4282782', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 5, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARCOIRIS/6-ARCO-IRIS.jpg'),
('af252477-3720-4b2d-b883-1cbd63f3a938', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 6, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARCOIRIS/7-ARCO-IRIS.jpg'),
('419e42e2-caa5-48ed-bde7-60288ca6c3bf', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 7, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARCOIRIS/8-ARCO-IRIS.jpg'),
('e973ad18-e77d-42ca-a225-fbcb6bbbfba8', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 8, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARCOIRIS/9-ARCO-IRIS.jpg'),
('c31c72d8-3e59-46f6-8dd3-41a3a28c35bd', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 9, 'http://correpalabrasprd.ulima.edu.pe/V2/story/pages/ARCOIRIS/10-ARCO-IRIS.jpg'),
('49e12e2a-f42e-4ff4-89e8-96bbfc5fe19c', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 10, 'http://correpalabrasprd.ulima.edu.pe/V2/story/vistafinal.jpg');
*/
-- Insertar datos en la tabla PageContent
INSERT INTO public."PageContent" ("Id", "PageId", "LanguageId", "CountWords", "Content") VALUES
('e73cedb9-cc80-497b-a571-c835d632ff71', '6357df8a-e4c0-4026-adce-c6fb6b2fe693', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 10, 'Tito cree que está en la mesa, bajo su tambor.'),
('92bd8848-716f-4c54-ac95-0b4703dc08ea', 'a2048d27-feb0-4de1-ab6a-e93a80cd0cdb', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 8, 'La busca también bajo su gorra y guantes.'),
('ad77a8f7-d0e8-458c-b4b6-40f9e78f1eb2', '3aee8bf5-7292-4ff7-9ce9-04eb6b043723', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 7, '¿Estará oculta bajo sus cuadernos o libros?'),
('a43c62cf-0d3a-4fdf-86ea-9b62b600928d', '673e1721-0b1e-4d01-a46d-9413d5fe3d6c', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 7, 'Seguramente está bajo su taza o plato.'),
('aa4dc8f8-b640-4f80-b2ba-f5ea9f5e2bdd', 'ce729a7c-18ab-4dc6-a91c-24e68c9bf372', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 9, 'En eso, recuerda que la puso en otro lugar.'),
('b0adb6b6-956b-4296-87b9-1d476c797e97', '11b83be1-9b1e-47ea-8645-84e2cad14247', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 9, 'Tito piensa: “Debo ser más ordenado y menos olvidadizo”.'),
('9dd98abe-94ee-4d59-a03e-aaa7de761d01', 'eeee1070-43e9-4d1e-99f7-f68dd024a18c', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 3, '¡Me gusta leer!'),

('4837d592-78f2-4b3b-817b-63a142955f6c', '6357df8a-e4c0-4026-adce-c6fb6b2fe693', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 7, 'Titum nin, mesapim kachkan tinyay ukupi, nispa.'),
('eef11af5-b1e3-4427-8edf-ed39710601ed', 'a2048d27-feb0-4de1-ab6a-e93a80cd0cdb', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 5, 'Maskantaqmi chukun hinallataq guantesnin ukupi.'),
('82283fd9-d9fe-4b72-a6a5-da54f0b1ad2d', '3aee8bf5-7292-4ff7-9ce9-04eb6b043723', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 7, '¿Icha pakasqa kachkan cuadernunkuna utaq librunkuna ukupi?'),
('3e9d7ef6-13da-4876-a0b4-d13fa52c523f', '673e1721-0b1e-4d01-a46d-9413d5fe3d6c', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 5, 'Tazan utaq platun ukupicha kachkan.'),
('1d8811d4-261c-44c1-8b49-940247d50bac', 'ce729a7c-18ab-4dc6-a91c-24e68c9bf372', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 4, 'Chaynapim, maypi churasqanta yuyarirqun.'),
('de03ed89-c0c6-4fb7-b3f8-4973eb612055', '11b83be1-9b1e-47ea-8645-84e2cad14247', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 7, 'Titum yuyaymanakun: “Ordenadum kanay hinallataq mana qunqaq”.'),
('4dd34f5c-284b-46dd-b66c-3a3194c8482b', 'eeee1070-43e9-4d1e-99f7-f68dd024a18c', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 3, '¡Qillqakuna ñawinchaymi kusichiwan!');

/*INSERT INTO public."PageContent" ("Id", "PageId", "LanguageId", "CountWords", "Content") VALUES
('a2fe1a22-737d-4ba0-ac83-cfd49f54c866', 'b1e58e18-6bf8-42c3-91bb-53d265962dd5', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 21, 'Un camión cruza a toda velocidad y casi choca con un automóvil. El conductor del auto piensa: “¡Qué chofer tan imprudente!”.'),
('557812a7-d8a6-4575-891a-fbe491169ad3', '4e027afd-25f3-4c8b-a86c-8ed407c887e8', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 19, 'Luego, el auto invade una ciclovía y casi atropella a una ciclista. La chica piensa: “¡Qué conductor tan imprudente!”.'),
('0452dfa7-ced2-434e-9242-acd5c2576881', 'ef08656d-fe03-46da-bcce-9b612bf533fc', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 22, 'Después, la ciclista decide ir por la vereda y casi se estrella con un peatón. El señor piensa: “¡Qué ciclista tan imprudente!”.'),
('d701cd21-9e0a-40e2-a092-f25268f9e776', 'e9f6ebc5-c2b8-46db-867b-1a52e13a11ba', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 20, 'Más adelante, el señor cruza la pista corriendo para ganarle a un camión. El chofer piensa: “¡Qué peatón tan imprudente!”.'),
('3a63d8ff-3c82-4daa-9f88-26d85b3dde90', 'd1c3f04e-67df-4e7e-9655-fc3cdf6551a8', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 3, '¡Me gusta leer!'),

('77f5d141-463c-4fed-a511-f2f43a70bca2', 'b1e58e18-6bf8-42c3-91bb-53d265962dd5', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 17, 'Huk camionmi utqayllaña rin hinaspa yaqalla huk automovilta tanqarqun. Auto apaqmi kaynata yuyaymanakun: “¡Mana yuyayniyuqmi chay choferqa!”.'),
('3632d978-8f2a-48fc-876c-743b2b1a56e1', '4e027afd-25f3-4c8b-a86c-8ed407c887e8', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 15, 'Chaymantaqa autom cicloviaman yaykurqun hinaspa yaqalla ciclistata sarurqun. Sipasmi yuyaymanakun: “¡Mana yuyayniyuqmi chay auto apaq!”.'),
('e4fa3034-5461-4680-acbc-60c3ecf42ef6', 'ef08656d-fe03-46da-bcce-9b612bf533fc', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 17, 'Chaymantaqa ciclistam veredanta riyta munan hinaspa yaqalla chuqarqun huk purikuqta. Chay taytam yuyaymanakun: “¡Mana yuyayniyuqmi chay ciclistaqa!”.'),
('3496635b-3405-42af-9b85-8e34b2c4064f', 'e9f6ebc5-c2b8-46db-867b-1a52e13a11ba', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 16, 'Chaymantañataqmi chay tayta raskillaña pistata chimpan huk camionta llallinampaq. Camión apaqmi yuyaymanakun: “¡Mana yuyayniyuqmi chay purikuqqa!”.'),
('13f1cb42-6487-4506-80e7-e9c5c1c65987', 'd1c3f04e-67df-4e7e-9655-fc3cdf6551a8', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 3, '¡Qillqakuna ñawinchaymi kusichiwan!');

INSERT INTO public."PageContent" ("Id", "PageId", "LanguageId", "CountWords", "Content") VALUES
('73f5b8f4-a025-4a53-a53e-d59d7fdaff67', 'dff7998a-47b6-4553-b589-4df655b94b13', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 14, 'Había una vez, en lo profundo del bosque, un árbol que era muy presumido.'),
('bf58e531-f029-4f57-a931-ef15bd8a76fa', '0b48cd67-5d13-43ce-90a2-a4a1e7e989d4', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 13, 'El árbol decía: “Yo tengo muchos frutos y ustedes solo tienen 5 frutos”.'),
('d0d983cb-0f8c-405a-9e4f-978091848281', '3e68b936-4075-4d50-ac05-fa9cfadaa34f', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 16, 'Llegó un día en que dejó de llover y el pobre árbol dejó de producir frutos.'),
('bea297d7-e17a-4c29-99ad-8a679810664b', 'cba205d0-0b12-4d0c-8081-a1ac98b9180a', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 17, 'Entonces, se sintió triste y se disculpó con los demás. Y prometió que ya no sería presumido.'),
('f00d9671-6070-44a0-b0f1-bd12d6b983ec', '60f103ff-95fd-47c1-8d3a-599f3dde7564', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 3, '¡Me gusta leer... a mi propio ritmo!');

INSERT INTO public."PageContent" ("Id", "PageId", "LanguageId", "CountWords", "Content") VALUES
('3d3bc0e6-4575-4b36-8d0a-032c80056346', 'a65097f9-b4d5-42c8-9323-d39f514721ef', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 7, 'El sol brillaba feliz en el cielo.'),
('9aaafb3e-5bad-44f7-abf8-ef4730846fc4', '27dd0b2f-09b3-4487-9d72-d991b566975c', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 5, 'De pronto, apareció una nube.'),
('1e4bb74c-f05a-4be9-b541-4093f0e48656', 'c8b0eadb-f400-4896-9bdc-908110a5d658', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 6, 'La nube decidió acercarse al sol.'),
('ae0648c4-dcd0-40f9-8875-1fd5bd1ba95c', '0bac75a0-c35f-4223-bae3-03468437e5af', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 7, 'El sol dijo \"¡No te acerques más!\".'),
('c08a945c-943b-467b-9032-ceec0f3eb0cc', '93432cd7-29cb-4424-9478-c8b1c4282782', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 10, 'La nube no le hizo caso y tapó al sol.'),
('08dced23-88eb-4f8d-8f7f-c1ec98f02f2a', 'af252477-3720-4b2d-b883-1cbd63f3a938', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 8, 'El sol y la nube empezaron a pelear.'),
('e9a9bf1f-da2b-4d02-8312-7972f81f3971', '419e42e2-caa5-48ed-bde7-60288ca6c3bf', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 7, 'La montaña les dijo: “¡Vivan en paz!”.'),
('c16f1665-b2de-4dd8-ad8f-dc5c51c8c118', 'e973ad18-e77d-42ca-a225-fbcb6bbbfba8', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 8, 'El sol y la nube se hicieron amigos.'),
('5e7c04a9-45de-4a43-82be-4bb81d5bc3fe', 'c31c72d8-3e59-46f6-8dd3-41a3a28c35bd', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 7, 'Y juntos crearon un bello arco iris.'),
('87abd355-630a-4eab-bc15-e414f9593c91', '49e12e2a-f42e-4ff4-89e8-96bbfc5fe19c', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062', 3, '¡Me gusta leer!'),

('5b9036cb-cb7a-4a51-b565-0541f67ba119', 'a65097f9-b4d5-42c8-9323-d39f514721ef', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 5, 'Intim hanaq pachapi kusisqa kancharichkarqa'),
('411a090b-1fb5-40a7-ab2e-c71f5c67a499', '27dd0b2f-09b3-4487-9d72-d991b566975c', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 3, 'Qunqayllamanta puyu rikurirqamun.'),
('3f910c84-a499-412a-b77d-219ddc2cf549', 'c8b0eadb-f400-4896-9bdc-908110a5d658', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 4, 'Hinaptinmi puyu intiman asuykun.'),
('982b4b5d-1eb6-4af4-86b4-f0a6b2f3ac79', '0bac75a0-c35f-4223-bae3-03468437e5af', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 5, 'Inti kaynata nin: “¡Amañana asuykamuyñachu!”.'),
('f8e4a06d-d550-468c-a02f-3b3bb6a251cf', '93432cd7-29cb-4424-9478-c8b1c4282782', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 5, 'Puyu, mana uyarispan intita harkarqun.'),
('e8555bc4-877b-4c23-8a84-1072f4290351', 'af252477-3720-4b2d-b883-1cbd63f3a938', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 4, 'Inti puyuwan maqanakuyta qallaykunku.'),
('65204220-7ffb-423f-a74d-10f768e4d84d', '419e42e2-caa5-48ed-bde7-60288ca6c3bf', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 6, 'Urqu paykunata kaynata nin: “¡Hawkalla kawsakuychik!”.'),
('683f835b-effa-4699-9396-f2adbdec77d1', 'e973ad18-e77d-42ca-a225-fbcb6bbbfba8', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 4, 'Intim killawan masichantinña kanku.'),
('1f1991c0-8f87-405f-9360-e611394f2dcd', 'c31c72d8-3e59-46f6-8dd3-41a3a28c35bd', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 4, 'Kuska sumaq chirapata ruwarqunku.'),
('b6a267fa-ac02-4f74-8892-04f5b8516325', '49e12e2a-f42e-4ff4-89e8-96bbfc5fe19c', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2', 3, '¡Qillqakuna ñawinchaymi kusichiwan!');
*/
-- Insertar datos en la tabla StoryLanguage
INSERT INTO public."StoryLanguage" ("Id", "StoryId", "LanguageId") VALUES
('86d40191-fb63-476e-a352-c60485ae30a4', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062'),
('398879b8-b361-48f7-8b47-919846fa5de9', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2');
/*
INSERT INTO public."StoryLanguage" ("Id", "StoryId", "LanguageId") VALUES
('daefd85c-4d1f-4796-8871-b955de23af9e', '975261fb-33f7-40b1-8afe-2390438ff977', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062'),
('88f12212-53a5-44c2-9ccd-59be0e071b43', '975261fb-33f7-40b1-8afe-2390438ff977', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2');

INSERT INTO public."StoryLanguage" ("Id", "StoryId", "LanguageId") VALUES
('0fdbdca6-7d56-4a1e-ace9-2d11d6d0d06f', 'c0f1d1cf-924b-46c3-95b1-d1657faec77c', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062');

INSERT INTO public."StoryLanguage" ("Id", "StoryId", "LanguageId") VALUES
('66c58d4f-3ed8-43c4-9790-bf72127d6374', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', '5dde49e2-3b87-49fa-9c91-6f85ae7d0062'),
('4065d4d4-6230-4845-a7de-41bb123f7d44', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', 'd16f4bbb-ba33-4500-a5aa-371b450ea6f2');
*/
-- Insertar datos en la tabla Category
INSERT INTO public."Category" ("Id", "Name", "Code", "UpdatedAt", "CategoryOrder", "Counter") VALUES
('2b3f995a-eb3b-4fa1-986f-63533e2e470c', 'Invitados', 'INV', '2019-09-30 08:18:34.48', 0, 0),
('2d3facb3-5d98-4a31-bb49-fe29e9d72eda', 'Todos', 'ALL', '2019-09-30 08:18:34.48', 1, 0),
('51397478-d5b3-4ede-b221-5f149cef17d6', 'Más leídos', 'MOSTREAD', '2019-09-30 08:18:34.48', 2, 0),
('fdebdade-00b0-48c2-b6ac-770eb33d2b3e', 'Nuevos', 'NEW', '2019-09-30 08:18:34.48', 3, 0),
('57071374-8784-43ed-b26d-76924d252674', 'Clásicos', 'CLA', '2019-09-30 08:18:34.48', 4, 0),
('9d9bddf7-e27f-4a27-b88c-98ea3cbcccb3', 'Sagas', 'SAGA', '2019-09-30 08:18:34.48', 5, 0),
('0d2661fc-7041-4cb1-8292-dcedad3bec7a', 'Escuelas', 'SCH', '2019-09-30 08:18:34.48', 6, 0),
('105a8d7b-20f1-4fdb-bab7-29fefcd115da', 'Regionales', 'REG', '2019-09-30 08:18:34.48', 7, 0);

-- Insertar datos en la tabla StoryCategory
INSERT INTO public."StoryCategory" ("Id", "StoryId", "CategoryId") VALUES
('2b5129c1-9542-42af-8fa0-e26aea76b655', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', '2d3facb3-5d98-4a31-bb49-fe29e9d72eda'),
('f161864e-538c-4d24-9630-c004a0664534', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', '2b3f995a-eb3b-4fa1-986f-63533e2e470c'),
('1d083b56-30b6-45a5-a330-0805ffc14829', '975261fb-33f7-40b1-8afe-2390438ff977', '2d3facb3-5d98-4a31-bb49-fe29e9d72eda'),
('128422e3-49b3-4c6f-878c-5c64885f93ad', 'c0f1d1cf-924b-46c3-95b1-d1657faec77c', '2d3facb3-5d98-4a31-bb49-fe29e9d72eda'),
('600089db-cbd3-41c3-81bc-ccca406fc12f', 'e02280f5-9d2d-42dd-b9b9-d40f68ca2c98', '2d3facb3-5d98-4a31-bb49-fe29e9d72eda');

-- Insertar datos en la tabla Avatar
INSERT INTO public."Avatar" ("Id", "StoryId", "AvatarUrl") VALUES
('6a5c94af-46d3-448c-8a23-5d966ffec626', '8b29f0b0-ba85-400a-8e38-4996fdc82bbf', 'avatar1.png'),
('137ceadf-2c37-44f7-a5fe-979b53db370d', '975261fb-33f7-40b1-8afe-2390438ff977', 'avatar2.png');

-- Insertar datos en la tabla Badge
INSERT INTO public."Badge" ("Id", "Name", "BadgeUrl") VALUES
('678eded8-f420-459b-a126-0ac4ec9aaf5e', 'Newcomer', 'badge1.png'),
('0f7fc983-9ab7-48db-974f-e402d681f4bc', 'Explorer', 'badge2.png');

-- Insertar datos en la tabla User
INSERT INTO public."User" ("Id", "UserType", "Name", "Email", "Password", "UpdatedAt", "VerificationCode", "CodeRegisteredDate", "CodeExpirationDate")
VALUES
('7267d866-4506-4983-8143-1dbe25216700', 0, 'coca', 'guest', '$2a$11$a71ErcamtMJb7nJ8keoCKuuAGNbsxv6oQxHk5NHdxfuH4GkQH/4oa', NOW(), 123456, NOW(), NOW() + INTERVAL '1 day');

-- Insertar datos en la tabla Profile
INSERT INTO public."Profile" ("Id", "AvatarId", "Username", "Gender", "BirthDate", "UserId") VALUES
('01423c28-4035-48f8-82ee-187cb552e902', '6a5c94af-46d3-448c-8a23-5d966ffec626', 'hello', 'Male', '1990-01-01', '7267d866-4506-4983-8143-1dbe25216700');

-- Insertar datos en la tabla ProfileStory
INSERT INTO public."ProfileStory" ("Id", "StoryLanguageId", "ProfileId", "IsDownloaded", "IsRead", "StartTime", "EndTime") VALUES
('e82d7570-24d2-45de-9736-1c9bcf47f49e', '86d40191-fb63-476e-a352-c60485ae30a4', '01423c28-4035-48f8-82ee-187cb552e902', false, false, NOW(), NOW() + INTERVAL '1 hour');

-- Insertar datos en la tabla UnlockedAvatar
INSERT INTO public."UnlockedAvatar" ("Id", "ProfileId", "AvatarId") VALUES
('1273c929-2113-4a44-ba63-b9f4736b62d4', '01423c28-4035-48f8-82ee-187cb552e902', '6a5c94af-46d3-448c-8a23-5d966ffec626');

-- Insertar datos en la tabla UnlockedBadge
INSERT INTO public."UnlockedBadge" ("Id", "ProfileId", "BadgeId") VALUES
('17270121-8b25-4774-b9d1-e614ab0fcc2e', '01423c28-4035-48f8-82ee-187cb552e902', '678eded8-f420-459b-a126-0ac4ec9aaf5e');