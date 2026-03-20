# Cloudflare R2 Setup Guide

## 1. Create a Cloudflare Account
- Visit [cloudflare.com](https://dash.cloudflare.com/sign-up)
- Sign up and verify your email

## 2. Enable R2 Storage
- Navigate to **R2 Object Storage** in the left sidebar
- Click **Get Started** (R2 has a generous free tier: 10 GB/month)

## 3. Create a Bucket
- Click **Create bucket**
- Name: `smart-insurance-hub`
- Region: Choose closest to your users (e.g., `apac` for India)
- Click **Create**

## 4. Generate API Keys
- Go to **R2** → **Manage R2 API Tokens**
- Click **Create API Token**
- Permissions: **Object Read & Write**
- Specify bucket: `smart-insurance-hub`
- Click **Create API Token**
- **Save** the Access Key ID and Secret Access Key (shown only once)

## 5. Get Your Account ID
- Go to Cloudflare Dashboard → Overview
- Your **Account ID** is shown on the right sidebar
- Your R2 endpoint is: `https://<account_id>.r2.cloudflarestorage.com`

## 6. Configure Environment Variables
Edit your `.env` file:
```env
CLOUDFLARE_R2_ACCESS_KEY=your_access_key_id
CLOUDFLARE_R2_SECRET_KEY=your_secret_access_key
CLOUDFLARE_R2_BUCKET=smart-insurance-hub
CLOUDFLARE_R2_ENDPOINT=https://your_account_id.r2.cloudflarestorage.com
```

## 7. Local Development (No R2)
The app includes a **local fallback** — if R2 keys are not configured, files are saved to `wwwroot/uploads/`. This lets you develop and test without a Cloudflare account.
