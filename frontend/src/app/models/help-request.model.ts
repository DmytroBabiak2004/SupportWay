import { Post } from './post.model';

// Основна модель (Read DTO)
export interface HelpRequest extends Post {
  // Поля, специфічні для запиту
  locationId: string;
  locationName: string;      // Назва району (наприклад, "Київський")
  statusName: string;        // Назва статусу (наприклад, "Активний")
  totalPayments: number;     // Сума зібраних коштів
}

// Модель для dropdown (Локація)
export interface LocationOption {
  id: string;
  districtName: string;
}

// Модель для dropdown (Статус)
export interface RequestStatusOption {
  id: string;
  nameOfStatus: string;
}
