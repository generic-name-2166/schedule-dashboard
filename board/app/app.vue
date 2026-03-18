<template>
  <div>
    <NuxtRouteAnnouncer />
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </div>
</template>

<style lang="css">
* {
  font-family: Helvetica, Arial, sans-serif;
}

:root {
  --primary-color: #eee;
  --secondary-color: rgba(238, 238, 238, 0.5);
  --primary-background: #1e2025;
  --secondary-background: #111214;
  --tertiary-background: #0a0b0d;
  --highlight-color: #d2f1e4;
}

body {
  margin: 0;
  padding: 0;
  color: var(--primary-color);
  background-color: var(--primary-background);
}
</style>
