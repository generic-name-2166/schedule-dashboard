import { defineStore } from "pinia";
import { reactive } from "vue";

export interface NotifierMessage {
  text: string;
  type: "info" | "error";
}

export const useNotifier = defineStore("notifier", () => {
  const messages = reactive<Map<number, NotifierMessage>>(new Map());

  const addMessage = (text: string, type: NotifierMessage["type"]) => {
    const timestamp: number = Date.now();
    messages.set(timestamp, { text, type });
    setTimeout(() => {
      messages.delete(timestamp);
    }, 5000);
  };

  return {
    messages,
    addMessage,
  };
});
