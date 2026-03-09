import js from "@eslint/js";
import { defineConfig } from "eslint/config";
import tseslint from "typescript-eslint";
import tsParser from "@typescript-eslint/parser";
import vue from "eslint-plugin-vue";
import prettier from "eslint-config-prettier";
import glob from "globals";

export default defineConfig(
  js.configs.recommended,
  ...tseslint.configs.recommended,
  ...tseslint.configs.strictTypeChecked,
  ...tseslint.configs.stylisticTypeChecked,
  ...vue.configs["flat/recommended"],
  {
    languageOptions: {
      parserOptions: {
        parser: tsParser,
        project: "./tsconfig.json",
        extraFileExtensions: [".vue"],
        sourceType: "module",
      },
      // https://github.com/eslint/eslint/discussions/17101#discussioncomment-5688967
      globals: {
        ...glob.browser,
        myCustomGlobal: "readonly",
      },
    },
    rules: {
      // i know what i'm doing
      "@typescript-eslint/no-non-null-assertion": "off",
    },
  },
  {
    files: ["**/*.vue"],
    languageOptions: {
      parserOptions: {
        parser: tsParser,
        project: "./tsconfig.json",
        extraFileExtensions: [".vue"],
        sourceType: "module",
      },
    },
    rules: {
      "vue/multi-word-component-names": "off",
      // https://github.com/vuejs/language-tools/issues/47
      "@typescript-eslint/no-unused-vars": "off",
    },
  },
  prettier,
  {
    ignores: [
      ".git/*",
      ".gitignore",
      "node_modules/*",
      ".nuxt/*",
      ".output/*",
      "dist/*",
      "**/bun.lock",
      "**/public/*",
      "**/eslint.config.js",
      "**/nuxt.config.ts",
    ],
  },
);
