export interface Chat {
  id: string;
  name?: string;
  userChats?: {
    userId: string;
    user: {
      userName: string;
    };
  }[];
  participants?: { id: string; userName: string }[];
}
