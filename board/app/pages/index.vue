<script setup lang="ts">
import Schedule from "../components/Schedule.vue";
import DataDate from "~/components/DataDate.vue";
import KebabMenu from "~/components/menu/KebabMenu.vue";
import KebabMenuLink from "~/components/menu/KebabMenuLink.vue";
import KebabMenuButton from "~/components/menu/KebabMenuButton.vue";
</script>

<template>
  <div class="heading">
    <h1>График строительства</h1>
    <div>
      <DataDate />
      <KebabMenu>
        <KebabMenuLink to="admin">Создать</KebabMenuLink>
        <KebabMenuLink to="admin?edit=">Редактировать</KebabMenuLink>
        <KebabMenuButton>Удалить</KebabMenuButton>
      </KebabMenu>
    </div>
  </div>
  <Schedule />
</template>

<style lang="css" scoped>
.heading {
  display: flex;
  justify-content: space-between;

  > div {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
}

.kebab {
  border: 0;
  margin: 0;
  padding: 0;
  background-color: transparent;
  cursor: pointer;
}

.popover {
  inset: auto;
  position: absolute;
  position-area: bottom left;
  transform: translate(0, -1rem);
  padding: 0;
  margin: 0;
  border: 0;
  background: transparent;
}

.menu {
  list-style-type: none;
  font-weight: bold;
  color: var(--primary-color);
  background-color: var(--tertiary-background);
  margin: 0;
  padding: 0.5rem 0;
  border: 0;
  border-radius: 1rem;
  display: flex;
  flex-direction: column;

  > li {
    display: block;
    margin: 0;
    padding: 0;
    width: stretch;
    height: stretch;

    &:not(:last-child) {
      border-bottom: 1px solid var(--secondary-color);
    }

    > .link {
      display: block;
      text-decoration: none;
      color: inherit;
      background-color: inherit;
      border: 0;
      margin: 0;
      padding: 0.5rem 1rem;
      font-size: inherit;
      font-weight: inherit;

      &:visited {
        color: inherit;
      }
    }
  }
}
</style>
