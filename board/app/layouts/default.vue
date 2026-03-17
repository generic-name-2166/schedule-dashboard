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
  width: stretch;
}

.content {
  padding-inline: 0.5rem;
  width: max(80%, 1200px);

  @media screen and (max-width: 1200px) {
    width: stretch;
  }
}
</style>
