<script setup lang="ts">
import Schedule from "../components/Schedule.vue";

const query = `
  query {
    scheduleObjects {
      id
      level
      wbsCode
      code
      name
      start
      end
    }
  }
`;
const params = new URLSearchParams({
  query,
});
const url: string = window
  ? `/graphql?${params}`
  : `http://localhost:5095/graphql?${params}`;

const response = await fetch(url).then((r) => r.json());
</script>

<template>
  <div class="heading">
    <h1>График строительства</h1>
    <div class="date">
      <p>
        Данные на
        <time datetime="2026-02-25T00:00:00Z">2026-02-25T00:00:00Z</time>
      </p>
      <button type="button" class="kebab">
        <svg
          width="60px"
          height="60px"
          viewBox="0 0 24 24"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
        >
          <circle cx="12" cy="7" r="1.5" fill="#000000" />
          <circle cx="12" cy="12" r="1.5" fill="#000000" />
          <circle cx="12" cy="17" r="1.5" fill="#000000" />
        </svg>
      </button>
      <menu class="menu">
        <li>
          <RouterLink to="admin">Create new</RouterLink>
        </li>
        <li>
          <RouterLink to="admin?edit=">Edit</RouterLink>
        </li>
        <li>
          <button type="button">Delete</button>
        </li>
      </menu>
    </div>
  </div>
  <Schedule :tasks="response.data.scheduleObjects" />
</template>

<style lang="postcss" scoped>
.heading {
  display: flex;
}

.date {
  display: flex;
}

.kebab {
  border: 0;
  margin: 0;
  padding: 0;
  background-color: transparent;
}

.menu {
  list-style-type: none;
}
</style>
