export interface Profile {
  profileId: string;
  userId: string;
  username: string;
  name?: string;
  fullName?: string;
  description: string;
  photoBase64?: string;
  createdAt: string;

  rating?: number;
  followersCount: number;
  followingCount: number;
  isOwnProfile: boolean;
}

