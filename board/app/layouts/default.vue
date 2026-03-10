<template>
  <div class="wrapper">
    <div class="content">
      <slot />
    </div>
  </div>
</template>

<style lang="css" scoped>
.wrapper {
  display: flex;
  justify-content: center;
}

.content {
  max-width: max(80%, 1200px);
}
</style>
