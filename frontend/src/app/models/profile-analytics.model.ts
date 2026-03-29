export interface AnalyticsPoint {
  date: string;
  count: number;
}

export interface AnalyticsTypeItem {
  typeId: string;
  typeName: string;
  itemsCount: number;
  totalQuantity: number;
}

export interface AnalyticsItem {
  itemName: string;
  count: number;
  totalQuantity: number;
}

export interface ProfileAnalytics {
  totalPosts: number;
  totalRequests: number;
  totalRequestItems: number;

  postsByDate: AnalyticsPoint[];
  requestsActivity: AnalyticsPoint[];

  requestsByType: AnalyticsTypeItem[];
  itemsBySupportType: AnalyticsTypeItem[];
  mostUsedItems: AnalyticsItem[];

  dominantRequestType?: AnalyticsTypeItem;
  mostRequestedItem?: AnalyticsItem;

  lastPostDate?: string;
  firstPostDate?: string;
  lastRequestDate?: string;
  firstRequestDate?: string;
}
