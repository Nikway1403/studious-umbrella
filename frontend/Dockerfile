FROM nginx:1.27-alpine

WORKDIR /usr/share/nginx/html

RUN rm -rf ./*

COPY src/pages/ ./
COPY src/js/ ./js/
COPY src/styles/ ./styles/

EXPOSE 80