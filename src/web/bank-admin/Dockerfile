# Stage 1: Build
FROM node:18-alpine AS build

WORKDIR /app

# Copy package files
COPY package.json ./
COPY package-lock.json ./

# Install dependencies
RUN npm ci

# Copy source files
COPY . .

# Build Angular app
RUN npm run build

# Stage 2: Runtime
FROM nginx:alpine AS runtime

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built Angular app
COPY --from=build /app/dist/bank-admin /usr/share/nginx/html

# Expose port
EXPOSE 80

ENTRYPOINT ["nginx", "-g", "daemon off;"]
