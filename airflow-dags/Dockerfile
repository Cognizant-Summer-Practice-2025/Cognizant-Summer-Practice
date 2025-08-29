FROM apache/airflow:3.0.4

# Install Chrome for Selenium (with proper architecture detection)
USER root
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    unzip \
    curl \
    && wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | gpg --dearmor -o /usr/share/keyrings/googlechrome-linux-keyring.gpg \
    && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/googlechrome-linux-keyring.gpg] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google-chrome.list \
    && apt-get update \
    && if [ "$(dpkg --print-architecture)" = "arm64" ]; then \
        echo "ARM64 detected - installing Chromium instead of Chrome"; \
        apt-get install -y chromium chromium-driver; \
    else \
        echo "AMD64 detected - installing Chrome"; \
        apt-get install -y google-chrome-stable; \
    fi \
    && rm -rf /var/lib/apt/lists/*

# Install ChromeDriver (only if not ARM64, since chromium-driver is installed for ARM64)
RUN if [ "$(dpkg --print-architecture)" != "arm64" ]; then \
        CHROME_DRIVER_VERSION=$(curl -sS chromedriver.storage.googleapis.com/LATEST_RELEASE) \
        && wget -O /tmp/chromedriver.zip "http://chromedriver.storage.googleapis.com/${CHROME_DRIVER_VERSION}/chromedriver_linux64.zip" \
        && unzip /tmp/chromedriver.zip chromedriver -d /usr/local/bin/ \
        && rm /tmp/chromedriver.zip \
        && chmod +x /usr/local/bin/chromedriver; \
    fi

USER airflow

# Install Python packages for advanced web scraping and AI
RUN pip install --no-cache-dir \
    feedparser \
    pendulum \
    openai \
    requests \
    beautifulsoup4 \
    trafilatura \
    newspaper3k \
    readability-lxml \
    selenium \
    lxml \
    html5lib \
    python-dateutil \
    urllib3
