export interface FaqBotRequest {
  question: string;
}

export interface FaqQuickAction {
  label: string;
  route: string;
}

export interface FaqBotResponse {
  answer: string;
  category: string;
  confidence: number;
  actions: FaqQuickAction[];
  suggestions: string[];
}

export interface FaqSuggestion {
  text: string;
  category: string;
}

export interface FaqChatMessage {
  id: string;
  sender: 'bot' | 'user';
  text: string;
  category?: string;
  actions?: FaqQuickAction[];
  suggestions?: string[];
  isLoading?: boolean;
}
