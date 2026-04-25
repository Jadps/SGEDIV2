

export interface UserDto {
    id?: string;
    email: string;
    firstName: string;
    lastName: string;
    secondLastName?: string | null;
    password?: string | null;
    tenantId?: string | null;
    roles: string[];
    friendlyName: string;
    phoneNumber?: string | null;
    catStatusAccountId: number;
    fullName?: string | null;
}
