<script setup lang="ts">
import { RouterLink } from "vue-router";
import Schedule from "../components/Schedule.vue";
import { useId } from "vue";

const menuId: string = useId();
</script>

<template>
  <div class="heading">
    <h1>График строительства</h1>
    <div class="date">
      <p>
        Данные предоставлены на
        <time datetime="2026-02-25T00:00:00Z">2026-02-25T00:00:00Z</time>
      </p>
      <button
        type="button"
        class="kebab"
        :popovertarget="menuId"
        :style="{
          anchorName: `--${menuId}`,
        }"
      >
        <svg
          width="60px"
          height="60px"
          viewBox="0 0 24 24"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
        >
          <circle cx="12" cy="7" r="1.5" fill="#FFF" />
          <circle cx="12" cy="12" r="1.5" fill="#FFF" />
          <circle cx="12" cy="17" r="1.5" fill="#FFF" />
        </svg>
      </button>
      <menu
        :id="menuId"
        popover
        class="menu"
        :style="{
          positionAnchor: `--${menuId}`,
        }"
      >
        <li>
          <RouterLink to="admin" class="link">Создать</RouterLink>
        </li>
        <li>
          <RouterLink to="admin?edit=" class="link">Редактировать</RouterLink>
        </li>
        <li>
          <button type="button" class="link">Удалить</button>
        </li>
      </menu>
    </div>
  </div>
  <Schedule />
</template>

<style lang="css" scoped>
.heading {
  display: flex;
  justify-content: space-between;
}

.date {
  display: flex;

  > p {
    margin: auto;
    font-weight: bold;
  }
}

.kebab {
  border: 0;
  margin: 0;
  padding: 0;
  background-color: transparent;
  cursor: pointer;
}

.menu {
  list-style-type: none;
  inset: auto;
  position: absolute;
  position-area: bottom left;
  font-weight: bold;
  color: var(--primary-color);
  background-color: var(--secondary-background);
  margin: 0;
  padding: 0.5rem 0;
  border: 0;
  border-radius: 1rem;

  > li {
    padding: 0.5rem 1rem;
    margin: 0;

    &:not(:last-child) {
      border-bottom: 1px solid var(--secondary-color);
    }

    > .link {
      text-decoration: none;
      color: inherit;
      background-color: inherit;
      border: 0;
      margin: 0;
      padding: 0;
      font-size: inherit;
      font-weight: inherit;

      &:visited {
        color: inherit;
      }
    }
  }
}
</style>
