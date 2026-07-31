FROM node:22-alpine AS build
WORKDIR /app
COPY frontend/healthcare-web/package.json frontend/healthcare-web/package-lock.json ./
RUN npm ci
COPY frontend/healthcare-web/ ./
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist/healthcare-web/browser /usr/share/nginx/html
COPY frontend/healthcare-web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
