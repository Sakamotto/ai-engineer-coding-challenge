FROM node:22-alpine

WORKDIR /app

# Copy manifests first for dependency caching
COPY frontend/package*.json ./
RUN npm install

# Copy all frontend files
COPY frontend/ ./

EXPOSE 5173

# Host mode listens on 0.0.0.0 enabling external access outside the Docker container
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]
