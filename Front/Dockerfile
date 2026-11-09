FROM node:22-alpine AS build

WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

ARG VITE_PEDIDOS_HUB_URL=/hubs/pedidos
ENV VITE_PEDIDOS_HUB_URL=${VITE_PEDIDOS_HUB_URL}

RUN npm run build

FROM nginx:1.27-alpine AS production

ENV API_PROXY_TARGET=http://host.docker.internal:5161
ENV NGINX_ENVSUBST_FILTER=API_PROXY_TARGET

COPY nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
