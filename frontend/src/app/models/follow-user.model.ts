export interface FollowUser {
  userId: string;
  username: string;
  name?: string;
  fullName?: string;
  photoBase64?: string | null;
  isVerified: boolean;
  verifiedAs?: number | null;
}
