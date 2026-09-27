# Interface language

`LocaleProvider` stores `en` or `vi` in `nomiwrite_locale`, defaults to Vietnamese, and synchronizes browser tabs. Language changes update context without replacing the page or editor. `SettingsButton` contains the language selector and the existing theme toggle.

Use `useLocale().t(source, parameters)` for interface copy. Add both translations to `messages.json`; source strings are keys, and existing bilingual aliases support shared labels. Keep interpolation parameters identical in both languages. Do not translate input values or values sent to APIs.

Exercise prompts, essay content, quiz answers, example sentences, and vocabulary terms bypass the catalog. Optional Vietnamese explanations supplied by the study-plan API are shown only in Vietnamese mode. Do not invent translations for data that the API does not provide.

Run `npm run test:i18n`, `npm run test:theme`, lint, and build after changes. These tests simulate storage and rendering contracts; they do not replace a browser check of the settings dialog, keyboard focus, and draft preservation.
