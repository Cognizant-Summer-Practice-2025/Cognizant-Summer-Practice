# Multi-stage build for Next.js messages service
FROM node:18-alpine AS base

# Install dependencies only when needed
FROM base AS deps
# Check https://github.com/nodejs/docker-node/tree/b4117f9333da4138b03a546ec926ef50a31506c3#nodealpine to understand why libc6-compat might be needed.
RUN apk add --no-cache libc6-compat
WORKDIR /app

# Install dependencies based on the preferred package manager
COPY package.json package-lock.json* ./
RUN npm ci

# Rebuild the source code only when needed
FROM base AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Next.js collects completely anonymous telemetry data about general usage.
# Learn more here: https://nextjs.org/telemetry
# Uncomment the following line in case you want to disable telemetry during the build.
ENV NEXT_TELEMETRY_DISABLED=1

# Build arguments for non-sensitive environment variables
ARG NEXT_PUBLIC_USER_API_URL
ARG NEXT_PUBLIC_PORTFOLIO_API_URL
ARG NEXT_PUBLIC_MESSAGES_API_URL
ARG NEXT_PUBLIC_AUTH_USER_SERVICE
ARG NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE
ARG NEXT_PUBLIC_MESSAGES_SERVICE
ARG NEXT_PUBLIC_ADMIN_SERVICE
ARG NEXT_PUBLIC_API_BASE_URL
ARG NEXT_PUBLIC_EMAILJS_SERVICE_ID
ARG NEXT_PUBLIC_EMAILJS_TEMPLATE_ID
ARG NEXT_PUBLIC_EMAILJS_PUBLIC_KEY

# Set non-sensitive environment variables for build
ENV NEXT_PUBLIC_USER_API_URL=$NEXT_PUBLIC_USER_API_URL
ENV NEXT_PUBLIC_PORTFOLIO_API_URL=$NEXT_PUBLIC_PORTFOLIO_API_URL
ENV NEXT_PUBLIC_MESSAGES_API_URL=$NEXT_PUBLIC_MESSAGES_API_URL
ENV NEXT_PUBLIC_AUTH_USER_SERVICE=$NEXT_PUBLIC_AUTH_USER_SERVICE
ENV NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE=$NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE
ENV NEXT_PUBLIC_MESSAGES_SERVICE=$NEXT_PUBLIC_MESSAGES_SERVICE
ENV NEXT_PUBLIC_ADMIN_SERVICE=$NEXT_PUBLIC_ADMIN_SERVICE
ENV NEXT_PUBLIC_API_BASE_URL=$NEXT_PUBLIC_API_BASE_URL
ENV NEXT_PUBLIC_EMAILJS_SERVICE_ID=$NEXT_PUBLIC_EMAILJS_SERVICE_ID
ENV NEXT_PUBLIC_EMAILJS_TEMPLATE_ID=$NEXT_PUBLIC_EMAILJS_TEMPLATE_ID
ENV NEXT_PUBLIC_EMAILJS_PUBLIC_KEY=$NEXT_PUBLIC_EMAILJS_PUBLIC_KEY

RUN npm run build

# Production image, copy all the files and run next
FROM base AS runner
WORKDIR /app

ENV NODE_ENV=development
# Uncomment the following line in case you want to disable telemetry during runtime.
ENV NEXT_TELEMETRY_DISABLED=1

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

COPY --from=builder /app/public ./public

# Set the correct permission for prerender cache
RUN mkdir .next
RUN chown nextjs:nodejs .next

# Automatically leverage output traces to reduce image size
# https://nextjs.org/docs/advanced-features/output-file-tracing
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs

EXPOSE 3002

ENV PORT=3002
ENV HOSTNAME="0.0.0.0"

# server.js is created by next build from the standalone output
# https://nextjs.org/docs/pages/api-reference/next-config-js/output
CMD ["node", "server.js"]
