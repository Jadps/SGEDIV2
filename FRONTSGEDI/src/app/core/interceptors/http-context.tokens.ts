import { HttpContextToken } from '@angular/common/http';

export const SILENCE_ERRORS = new HttpContextToken<boolean>(() => false);
