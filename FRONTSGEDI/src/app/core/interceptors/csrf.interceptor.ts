import { HttpInterceptorFn, HttpXsrfTokenExtractor } from '@angular/common/http';
import { inject } from '@angular/core';

export const csrfInterceptor: HttpInterceptorFn = (req, next) => {
    const tokenExtractor = inject(HttpXsrfTokenExtractor);
    const headerName = 'X-XSRF-TOKEN';

    const angularToken = tokenExtractor.getToken();

    const match = document.cookie.match(new RegExp('(^| )XSRF-TOKEN=([^;]+)'));
    const manualToken = match ? decodeURIComponent(match[2]) : null;

    const finalToken = angularToken || manualToken;

    if (finalToken && req.method !== 'GET' && req.method !== 'HEAD') {
        req = req.clone({
            headers: req.headers.set(headerName, finalToken)
        });
    }

    return next(req);
};
