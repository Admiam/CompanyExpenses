// /src/proxy/restClient.ts
// Simple REST client for use by a proxy. Uses global fetch (Node 18+ or browsers).
// Provides typed helpers, timeout, JSON handling and basic error wrapper.

export interface RequestOptions {
    headers?: Record<string, string>;
    query?: Record<string | number, string | number | boolean | undefined>;
    body?: any;
    timeoutMs?: number; // per-request timeout in ms
}

export interface RestClientOptions {
    baseUrl?: string;
    defaultHeaders?: Record<string, string>;
    timeoutMs?: number;
}

export class HttpError extends Error {
    public status: number;
    public statusText: string;
    public body: any;

    constructor(status: number, statusText: string, body: any) {
        super(`HTTP ${status} ${statusText}`);
        this.status = status;
        this.statusText = statusText;
        this.body = body;
    }
}

export class RestClient {
    private baseUrl: string;
    private defaultHeaders: Record<string, string>;
    private timeoutMs?: number;

    constructor(opts: RestClientOptions = {}) {
        this.baseUrl = opts.baseUrl ?? '';
        this.defaultHeaders = opts.defaultHeaders ?? { 'Accept': 'application/json' };
        this.timeoutMs = opts.timeoutMs;
    }

    private buildUrl(pathOrUrl: string, query?: RequestOptions['query']): string {
        // Allow passing a full URL or a path relative to baseUrl
        const isAbsolute = /^https?:\/\//i.test(pathOrUrl);
        const url = isAbsolute ? new URL(pathOrUrl) : new URL(pathOrUrl, this.baseUrl || undefined);
        if (query) {
            Object.entries(query).forEach(([k, v]) => {
                if (v === undefined) return;
                url.searchParams.set(String(k), String(v));
            });
        }
        return url.toString();
    }

    private async fetchWithTimeout(input: RequestInfo, init: RequestInit, timeoutMs?: number): Promise<Response> {
        const controller = new AbortController();
        const timer = (timeoutMs ?? this.timeoutMs) ? setTimeout(() => controller.abort(), timeoutMs ?? this.timeoutMs) : undefined;
        try {
            const resp = await fetch(input, { ...init, signal: controller.signal });
            return resp;
        } finally {
            if (timer) clearTimeout(timer);
        }
    }

    private async handleResponse<T>(res: Response): Promise<T> {
        const contentType = res.headers.get('content-type') || '';
        const isJson = contentType.includes('application/json') || contentType.includes('+json');

        let body: any;
        try {
            body = isJson ? await res.json() : await res.text();
        } catch (e) {
            body = undefined;
        }

        if (!res.ok) {
            throw new HttpError(res.status, res.statusText, body);
        }
        return body as T;
    }

    private mergeHeaders(headers?: Record<string, string>, body?: any): Record<string, string> {
        const out: Record<string, string> = { ...this.defaultHeaders, ...(headers ?? {}) };
        if (body != null && !(body instanceof FormData) && !out['Content-Type']) {
            out['Content-Type'] = 'application/json';
        }
        return out;
    }

    async request<T = any>(method: string, pathOrUrl: string, opts: RequestOptions = {}): Promise<T> {
        const url = this.buildUrl(pathOrUrl, opts.query);
        const headers = this.mergeHeaders(opts.headers, opts.body);

        const init: RequestInit = {
            method,
            headers,
        };

        if (opts.body != null) {
            if (opts.body instanceof FormData) {
                // let fetch set correct headers for FormData
                // remove content-type so browser/node sets correct boundary
                delete (init.headers as Record<string, string>)['Content-Type'];
                init.body = opts.body as any;
            } else if (typeof opts.body === 'string' || opts.body instanceof URLSearchParams || opts.body instanceof ArrayBuffer) {
                init.body = opts.body as any;
            } else {
                init.body = JSON.stringify(opts.body);
            }
        }

        const res = await this.fetchWithTimeout(url, init, opts.timeoutMs);
        return this.handleResponse<T>(res);
    }

    get<T = any>(pathOrUrl: string, opts?: Omit<RequestOptions, 'body'>): Promise<T> {
        return this.request<T>('GET', pathOrUrl, opts);
    }

    post<T = any>(pathOrUrl: string, body?: any, opts?: Omit<RequestOptions, 'body'> & { timeoutMs?: number; headers?: Record<string,string> }): Promise<T> {
        return this.request<T>('POST', pathOrUrl, { ...opts, body });
    }

    put<T = any>(pathOrUrl: string, body?: any, opts?: Omit<RequestOptions, 'body'> & { timeoutMs?: number; headers?: Record<string,string> }): Promise<T> {
        return this.request<T>('PUT', pathOrUrl, { ...opts, body });
    }

    delete<T = any>(pathOrUrl: string, opts?: Omit<RequestOptions, 'body'>): Promise<T> {
        return this.request<T>('DELETE', pathOrUrl, opts);
    }
}

// Default export for convenience. You can create new RestClient(...) if you need multiple instances.
export default new RestClient();