export interface LoginDto {
    email: string;
    password: string;
}

export interface AuthResponseDto {
    token: string;
    refreshToken: string;
    expiration: string;
}

export interface RefreshTokenRequestDto {
    refreshToken: string;
}

export interface ForgotPasswordDto {
    email: string;
}

export interface ResetPasswordDto {
    email: string;
    token: string;
    newPassword: string;
}
