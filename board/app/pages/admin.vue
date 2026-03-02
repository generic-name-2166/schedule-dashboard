<script setup lang="ts">
async function submit(event: SubmitEvent): Promise<void> {
  event.preventDefault();

  const form = event.target as HTMLFormElement;
  const inputs = new FormData(form);
  const file = inputs.get("file") as File;

  const operations: string = JSON.stringify({
    query: `
      mutation UploadFile($file: Upload!) {
        uploadFile(file: $file)
      }
    `,
    variables: { file: null },
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

  const result = await res.json();
  console.log(result);
}
</script>

<template>
  <form method="POST" action="/graphql" @submit="submit">
    <h1>upload</h1>
    <label> csv <input type="file" name="file" accept=".csv" /> </label>
    <button type="submit">submit</button>
  </form>
</template>
