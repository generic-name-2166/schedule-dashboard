<script setup lang="ts">
import { ref, useTemplateRef } from "vue";

const isDragover = ref(false);
const fileName = ref("");
const target = useTemplateRef<HTMLInputElement>("target");

async function submit(event: SubmitEvent): Promise<void> {
  event.preventDefault();

  const form = event.target as HTMLFormElement;
  const inputs = new FormData(form);
  const file = inputs.get("file") as File;
  const date = new Date(inputs.get("date") as string);

  const operations: string = JSON.stringify({
    query: `
      mutation UploadFile($date: DateTime!, $file: Upload!) {
        uploadFile(date: $date, file: $file)
      }
    `,
    variables: { date, file: null },
  });

  const map: string = JSON.stringify({
    "0": ["variables.file"],
  });

  const formData = new FormData();
  formData.append("operations", operations);
  formData.append("map", map);
  formData.append("0", file);

  const res = await fetch(form.action, {
    method: form.method,
    // Do NOT set Content-Type; let the browser set multipart
    body: formData,
    // https://stackoverflow.com/a/76686111
    headers: {
      "GraphQL-preflight": "1",
    },
  });

  // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
  const result = await res.json();
  console.log(result);
}

// https://css-tricks.com/drag-and-drop-file-uploading/
function drag(event: DragEvent): void {
  event.preventDefault();
  event.stopPropagation();
}

function dragover(event: DragEvent): void {
  event.preventDefault();
  event.stopPropagation();
  isDragover.value = true;
}

function dragleave(event: DragEvent): void {
  event.preventDefault();
  event.stopPropagation();
  isDragover.value = false;
}

const drop = (event: DragEvent): void => {
  dragleave(event);
  const file = event.dataTransfer?.files[0];
  if (!file) {
    fileName.value = "";
    return;
  }
  fileName.value = file.name;

  const dataTransfer = new DataTransfer();
  dataTransfer.items.add(file);

  target.value!.files = dataTransfer.files;
};

const input = (): void => {
  const file = target.value!.files?.[0];
  if (!file) {
    fileName.value = "";
    return;
  }
  fileName.value = file.name;
};
</script>

<template>
  <form method="POST" action="/graphql" @submit="submit">
    <div class="heading">
      <h1>График строительства</h1>

      <div>
        <label class="date"><input type="date" name="date" required /></label>
        <button type="submit">submit</button>
      </div>
    </div>

    <div class="upload">
      <div
        :class="{ 'is-dragover': isDragover }"
        @drag="drag"
        @drop="drop"
        @dragover="dragover"
        @dragenter="dragover"
        @dragleave="dragleave"
        @dragend="dragleave"
      >
        <div>
          <label class="input">
            <svg
              width="100"
              height="100"
              viewBox="0 0 40 40"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                d="M35 25V31.6667C35 32.5507 34.6488 33.3986 34.0237 34.0237C33.3986 34.6488 32.5507 35 31.6667 35H8.33333C7.44928 35 6.60143 34.6488 5.97631 34.0237C5.35119 33.3986 5 32.5507 5 31.6667V25M28.3333 13.3333L20 5M20 5L11.6667 13.3333M20 5V25"
                stroke="#eee"
                stroke-width="3.5"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
            </svg>
            <input
              ref="target"
              type="file"
              name="file"
              accept=".csv"
              required
              @change="input"
            />
          </label>
        </div>
        <p v-if="fileName.length === 0">
          Выберите CSV файл или перетащите его сюда
        </p>
        <p v-else>
          {{ fileName }}
        </p>
      </div>
    </div>
  </form>
</template>

<style lang="css" scoped>
.heading {
  display: flex;
  justify-content: space-between;
}

.date {
  display: flex;
  justify-content: center;
  align-items: center;
}

.upload {
  width: stretch;
  display: flex;
  justify-content: center;

  > div {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    border: 0.25rem solid var(--secondary-color);
    border-radius: 1rem;
    padding: 5rem;

    @media screen and (max-width: 800px) {
      width: auto;
    }
  }
}

.input {
  cursor: pointer;

  > input {
    display: none;
  }
}

.is-dragover {
  background-color: var(--secondary-background);
}
</style>
