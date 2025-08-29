"""
### Tech News Publisher DAG
This DAG performs a full ETL process with advanced web scraping:
1.  **Extract**: Scrapes news from multiple sources using advanced content extraction methods.
2.  **Transform**: Uses OpenRouter (OpenAI SDK) to generate summaries.
3.  **Load**: Prints the results to logs.

**Setup Required:**
- **Airflow Variable**: `OPENROUTER_API_KEY` containing your OpenRouter API key.
"""
import pendulum
import logging
import os
import json
import time
import re
import random
from typing import List, Dict, Optional
import requests
from bs4 import BeautifulSoup
import concurrent.futures
from urllib.parse import urlparse, urljoin
import feedparser

# Advanced content extraction libraries
import trafilatura
import newspaper
from readability import Document

# Browser automation for JavaScript-heavy sites
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import TimeoutException, WebDriverException

from openai import OpenAI
from airflow.decorators import dag, task
from airflow.models.variable import Variable


# OpenRouter integration for AI summarization
# client = OpenAI(
#     base_url="https://openrouter.ai/api/v1",
#     api_key="<OPENROUTER_API_KEY>",
# )
# completion = client.chat.completions.create(
#     extra_headers={
#         "HTTP-Referer": "<YOUR_SITE_URL>",
#         "X-Title": "<YOUR_SITE_NAME>",
#     },
#     extra_body={},
#     model="openai/gpt-oss-20b:free",
#     messages=[
#         {
#             "role": "user",
#             "content": "What is the meaning of life?"
#         }
#     ]
# )
# print(completion.choices[0].message.content)


@dag(
    dag_id="tech_news_publisher",
    start_date=pendulum.datetime(2024, 1, 1, tz="UTC"),
    schedule="@daily",
    catchup=False,
    tags=["scraper", "openrouter", "advanced"],
    doc_md=__doc__,
)
def tech_news_publisher_dag():
    
    class AdvancedContentExtractor:
        """Advanced content extraction using multiple methods and libraries."""
        
        def __init__(self):
            self.session = requests.Session()
            self.session.headers.update({
                "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8",
                "Accept-Language": "en-US,en;q=0.9",
                "Accept-Encoding": "gzip, deflate, br",
                "DNT": "1",
                "Connection": "keep-alive",
                "Upgrade-Insecure-Requests": "1",
                "Sec-Fetch-Dest": "document",
                "Sec-Fetch-Mode": "navigate",
                "Sec-Fetch-Site": "none",
                "Cache-Control": "max-age=0",
                "Pragma": "no-cache",
            })
            
        def extract_with_selenium(self, url: str, timeout: int = 30) -> Optional[str]:
            """Extract content using Selenium for JavaScript-heavy sites."""
            try:
                options = Options()
                options.add_argument('--headless')
                options.add_argument('--no-sandbox')
                options.add_argument('--disable-dev-shm-usage')
                options.add_argument('--disable-gpu')
                options.add_argument('--window-size=1920,1080')
                options.add_argument('--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36')
                options.add_argument('--disable-extensions')
                options.add_argument('--disable-web-security')
                options.add_argument('--allow-running-insecure-content')
                
                # Detect and configure browser
                driver = None
                try:
                    # Try Chromium first (for ARM64/Apple Silicon)
                    import subprocess
                    result = subprocess.run(['which', 'chromium'], capture_output=True, text=True)
                    if result.returncode == 0:
                        options.binary_location = '/usr/bin/chromium'
                        # Set chromium-driver path
                        from selenium.webdriver.chrome.service import Service
                        service = Service('/usr/bin/chromedriver')
                        driver = webdriver.Chrome(service=service, options=options)
                    else:
                        # Fall back to Chrome
                        driver = webdriver.Chrome(options=options)
                except Exception as e:
                    logging.warning(f"Failed to start Chrome/Chromium: {e}")
                    # Try with explicit service path
                    try:
                        from selenium.webdriver.chrome.service import Service
                        service = Service('/usr/local/bin/chromedriver')
                        driver = webdriver.Chrome(service=service, options=options)
                    except Exception as e2:
                        logging.error(f"All Chrome driver attempts failed: {e2}")
                        return None
                
                if not driver:
                    return None
                
                try:
                    driver.get(url)
                    # Wait for content to load
                    WebDriverWait(driver, timeout).until(
                        EC.presence_of_element_located((By.TAG_NAME, "article"))
                    )
                    
                    # Try to find main content
                    content_selectors = [
                        "article", 
                        ".article-content", 
                        ".post-content", 
                        ".entry-content",
                        "main",
                        ".content",
                        "#content"
                    ]
                    
                    for selector in content_selectors:
                        try:
                            element = driver.find_element(By.CSS_SELECTOR, selector)
                            text = element.text.strip()
                            if len(text) > 200:
                                return text
                        except:
                            continue
                    
                    # Fallback: get body text
                    body = driver.find_element(By.TAG_NAME, "body")
                    return body.text.strip()
                    
                finally:
                    driver.quit()
                    
            except Exception as e:
                logging.warning(f"Selenium extraction failed for {url}: {e}")
                return None
        
        def extract_with_trafilatura(self, url: str) -> Optional[str]:
            """Extract content using trafilatura."""
            try:
                # Use requests session to get content first
                response = self.session.get(url, timeout=30)
                response.raise_for_status()
                
                # Pass text content to trafilatura, not bytes
                text = trafilatura.extract(
                    response.text,
                    include_comments=False,
                    include_tables=True,
                    favor_recall=True,
                    no_fallback=False,
                    include_formatting=False,
                    target_language="en"
                )
                return text if text and len(text.strip()) > 200 else None
            except Exception:
                # Silently fail and let other methods handle it
                return None
        
        def extract_with_newspaper(self, url: str) -> Optional[str]:
            """Extract content using newspaper3k."""
            try:
                # Create newspaper config with headers
                config = newspaper.Config()
                config.browser_user_agent = self.session.headers.get('User-Agent', '')
                config.request_timeout = 30
                
                article = newspaper.Article(url, config=config)
                article.download()
                article.parse()
                
                if article.text and len(article.text.strip()) > 200:
                    return article.text.strip()
            except Exception as e:
                logging.warning(f"Newspaper extraction failed for {url}: {e}")
                return None
        
        def extract_with_readability(self, url: str) -> Optional[str]:
            """Extract content using python-readability."""
            try:
                response = self.session.get(url, timeout=30)
                response.raise_for_status()
                
                # Fix bytes/string issue - pass text content to Document
                doc = Document(response.text)
                html_content = doc.summary()
                
                if html_content:
                    soup = BeautifulSoup(html_content, 'html.parser')
                    text = soup.get_text(separator=' ', strip=True)
                    return text if text and len(text.strip()) > 200 else None
                    
            except Exception as e:
                logging.warning(f"Readability extraction failed for {url}: {e}")
                return None
        
        def extract_with_beautifulsoup(self, url: str) -> Optional[str]:
            """Extract content using BeautifulSoup with intelligent selectors."""
            try:
                response = self.session.get(url, timeout=30)
                response.raise_for_status()
                
                # Handle encoding properly to avoid replacement character issues
                response.encoding = response.apparent_encoding or 'utf-8'
                soup = BeautifulSoup(response.text, 'html.parser')
                
                # Remove unwanted elements
                for element in soup(['script', 'style', 'nav', 'header', 'footer', 'aside', 'advertisement']):
                    element.decompose()
                
                # Try content selectors in order of specificity
                content_selectors = [
                    'article',
                    '[role="main"]',
                    '.article-content',
                    '.post-content', 
                    '.entry-content',
                    '.content-body',
                    '.story-body',
                    '.article-body',
                    'main',
                    '.content',
                    '#content'
                ]
                
                for selector in content_selectors:
                    elements = soup.select(selector)
                    for element in elements:
                        text = element.get_text(separator=' ', strip=True)
                        if len(text) > 200:
                            return text
                
                # Final fallback: get all paragraph text
                paragraphs = soup.find_all('p')
                text = ' '.join([p.get_text(strip=True) for p in paragraphs])
                return text if len(text) > 200 else None
                
            except Exception:
                # Silently fail and let other methods handle it
                return None
        
        def extract_with_techcrunch_optimized(self, url: str) -> Optional[str]:
            """Optimized extraction specifically for TechCrunch articles."""
            try:
                # TechCrunch-specific headers to appear more like a real browser
                techcrunch_headers = {
                    'User-Agent': 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
                    'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
                    'Accept-Language': 'en-US,en;q=0.5',
                    'Accept-Encoding': 'gzip, deflate',
                    'Referer': 'https://techcrunch.com/',
                    'Connection': 'keep-alive',
                    'Upgrade-Insecure-Requests': '1',
                }
                
                response = self.session.get(url, headers=techcrunch_headers, timeout=30)
                response.raise_for_status()
                response.encoding = 'utf-8'
                
                soup = BeautifulSoup(response.text, 'html.parser')
                
                # TechCrunch-specific content selectors (in order of preference)
                techcrunch_selectors = [
                    'div.article-content',
                    'div.entry-content', 
                    'div[data-module="ArticleBody"]',
                    'div.post-content',
                    'article div.content',
                    'main article',
                    '.wp-block-post-content',
                    'div[class*="article"]',
                ]
                
                # Remove unwanted elements first
                for unwanted in soup(['script', 'style', 'nav', 'header', 'footer', 'aside', 
                                    'div[class*="ad"]', 'div[class*="newsletter"]', 
                                    'div[class*="related"]', 'div[class*="sidebar"]']):
                    unwanted.decompose()
                
                # Try TechCrunch-specific selectors
                for selector in techcrunch_selectors:
                    elements = soup.select(selector)
                    for element in elements:
                        # Clean up the content
                        for unwanted in element.find_all(['div', 'p'], class_=lambda x: x and any(
                            term in x.lower() for term in ['ad', 'newsletter', 'related', 'share', 'social']
                        )):
                            unwanted.decompose()
                        
                        text = element.get_text(separator=' ', strip=True)
                        if len(text) > 500:  # Higher threshold for TechCrunch
                            return text
                
                # Fallback: get all paragraph content
                paragraphs = soup.find_all('p')
                if paragraphs:
                    text = ' '.join([p.get_text(strip=True) for p in paragraphs if p.get_text(strip=True)])
                    if len(text) > 300:
                        return text
                        
                return None
                
            except Exception:
                return None

        def extract_full_content(self, url: str, source: str = "") -> Optional[str]:
            """Extract full content using multiple methods as fallbacks."""
            logging.info(f"Extracting content from {url} (Source: {source})")
            
            # For TechCrunch, use optimized method first
            if 'techcrunch.com' in url.lower():
                content = self.extract_with_techcrunch_optimized(url)
                if content:
                    logging.info(f"✓ TechCrunch-Optimized extracted {len(content)} chars from {url}")
                    return self.clean_content(content, source)
            
            # Method 1: BeautifulSoup (most reliable)
            content = self.extract_with_beautifulsoup(url)
            if content:
                logging.info(f"✓ BeautifulSoup extracted {len(content)} chars from {url}")
                return self.clean_content(content, source)
            
            # Method 2: Newspaper3k (very reliable for news sites)
            content = self.extract_with_newspaper(url)
            if content:
                logging.info(f"✓ Newspaper3k extracted {len(content)} chars from {url}")
                return self.clean_content(content, source)
            
            # Method 3: Readability
            content = self.extract_with_readability(url)
            if content:
                logging.info(f"✓ Readability extracted {len(content)} chars from {url}")
                return self.clean_content(content, source)
            
            # Method 4: Selenium (last resort, only if absolutely needed)
            if "theinformation.com" in url.lower():
                content = self.extract_with_selenium(url)
                if content:
                    logging.info(f"✓ Selenium extracted {len(content)} chars from {url}")
                    return self.clean_content(content, source)
            
            logging.warning(f"✗ All extraction methods failed for {url}")
            return None
        
        def clean_content(self, text: str, source: str) -> str:
            """Clean and normalize extracted content."""
            if not text:
                return ""
            
            # Remove common boilerplate patterns
            patterns_to_remove = [
                r"Subscribe to.*?newsletter",
                r"Follow us on.*?social media",
                r"Share this article",
                r"Read more(?:\s+articles)?:?",
                r"Continue reading",
                r"Sign up for.*?updates",
                r"Get the latest.*?news",
                r"This article originally appeared on.*?\.com",
                r"Image credit:.*",
                r"Photo credit:.*",
                r"Source:.*",
                r"Related:.*",
                r"See also:.*",
                r"Advertisement",
                r"ADVERTISEMENT",
                r"Sponsored content",
                r"\d+ comments?",
                r"Comments? \(\d+\)",
            ]
            
            for pattern in patterns_to_remove:
                text = re.sub(pattern, "", text, flags=re.IGNORECASE | re.DOTALL)
            
            # Site-specific cleanup
            if "TechCrunch" in source:
                text = re.sub(r"We're launching.*?TC Sessions.*?\.(?:\s|$)", "", text, flags=re.IGNORECASE)
                text = re.sub(r"Image Credits?:.*?(?:\.|$)", "", text, flags=re.IGNORECASE)
            elif "Engadget" in source:
                text = re.sub(r"This article originally appeared on Engadget.*", "", text, flags=re.DOTALL)
                text = re.sub(r"Image credit:.*?Engadget", "", text, flags=re.IGNORECASE)
            elif "Gizmodo" in source:
                text = re.sub(r"Image:.*?Gizmodo", "", text, flags=re.IGNORECASE)
                text = re.sub(r"Read more:.*", "", text, flags=re.DOTALL)
            elif "The Information" in source:
                text = re.sub(r"This story is available exclusively to.*", "", text, flags=re.IGNORECASE)
                text = re.sub(r"Subscribe to.*?The Information.*", "", text, flags=re.IGNORECASE)
            
            # Normalize whitespace
            text = re.sub(r'\s+', ' ', text.strip())
            text = re.sub(r'\n\s*\n', '\n\n', text)
            
            return text.strip()

    @task
    def scrape_techcrunch() -> list[dict]:
        """Advanced TechCrunch scraper with RSS + full content extraction."""
        logging.info("Scraping TechCrunch with advanced content extraction...")
        
        extractor = AdvancedContentExtractor()
        
        # Get articles from RSS feed
        feed_url = "https://techcrunch.com/feed/"
        feed = feedparser.parse(feed_url)
        articles = []
        
        for entry in feed.entries[:20]:
            article = {
                "title": getattr(entry, "title", ""),
                "link": getattr(entry, "link", ""),
                "summary": getattr(entry, "summary", ""),
                "published": getattr(entry, "published", ""),
                "guid": getattr(entry, "id", ""),
                "categories": [tag.term for tag in getattr(entry, "tags", []) if hasattr(tag, "term")],
                "source": "TechCrunch",
                "content": None
            }
            
            # Extract full content from the article URL
            if article["link"]:
                full_content = extractor.extract_full_content(article["link"], "TechCrunch")
                article["content"] = full_content or article["summary"]
            else:
                article["content"] = article["summary"]
            
            articles.append(article)
            
            # Rate limiting
            time.sleep(random.uniform(1, 2))
        
        logging.info(f"Successfully scraped {len(articles)} TechCrunch articles")
        return articles

    @task  
    def scrape_engadget() -> list[dict]:
        """Advanced Engadget scraper with RSS + full content extraction."""
        logging.info("Scraping Engadget with advanced content extraction...")
        
        extractor = AdvancedContentExtractor()
        
        # Get articles from RSS feed
        feed_url = "https://www.engadget.com/rss.xml"
        feed = feedparser.parse(feed_url)
        articles = []
        
        for entry in feed.entries[:20]:
            article = {
                "title": getattr(entry, "title", ""),
                "link": getattr(entry, "link", ""),
                "summary": getattr(entry, "summary", ""),
                "published": getattr(entry, "published", ""),
                "guid": getattr(entry, "id", ""),
                "categories": [tag.term for tag in getattr(entry, "tags", []) if hasattr(tag, "term")],
                "source": "Engadget",
                "content": None
            }
            
            # Extract full content from the article URL
            if article["link"]:
                full_content = extractor.extract_full_content(article["link"], "Engadget")
                article["content"] = full_content or article["summary"]
            else:
                article["content"] = article["summary"]
            
            articles.append(article)
            
            # Rate limiting
            time.sleep(random.uniform(1, 2))
        
        logging.info(f"Successfully scraped {len(articles)} Engadget articles")
        return articles

    @task
    def scrape_gizmodo() -> list[dict]:
        """Advanced Gizmodo scraper with RSS + full content extraction."""
        logging.info("Scraping Gizmodo with advanced content extraction...")
        
        extractor = AdvancedContentExtractor()
        
        # Get articles from RSS feed
        feed_url = "https://gizmodo.com/rss"
        feed = feedparser.parse(feed_url)
        articles = []
        
        for entry in feed.entries[:20]:
            article = {
                "title": getattr(entry, "title", ""),
                "link": getattr(entry, "link", ""),
                "summary": getattr(entry, "summary", ""),
                "published": getattr(entry, "published", ""),
                "guid": getattr(entry, "id", ""),
                "categories": [tag.term for tag in getattr(entry, "tags", []) if hasattr(tag, "term")],
                "source": "Gizmodo",
                "content": None
            }
            
            # Extract full content from the article URL
            if article["link"]:
                full_content = extractor.extract_full_content(article["link"], "Gizmodo")
                article["content"] = full_content or article["summary"]
            else:
                article["content"] = article["summary"]
            
            articles.append(article)
            
            # Rate limiting
            time.sleep(random.uniform(1, 2))
        
        logging.info(f"Successfully scraped {len(articles)} Gizmodo articles")
        return articles

    @task
    def scrape_the_information() -> list[dict]:
        """Advanced The Information scraper using browser automation."""
        logging.info("Scraping The Information with browser automation...")
        
        extractor = AdvancedContentExtractor()
        articles = []
        
        try:
            # Use Selenium to get article links from homepage
            options = Options()
            options.add_argument('--headless')
            options.add_argument('--no-sandbox') 
            options.add_argument('--disable-dev-shm-usage')
            options.add_argument('--disable-gpu')
            options.add_argument('--disable-extensions')
            options.add_argument('--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36')
            
            # Detect and configure browser
            driver = None
            try:
                # Try Chromium first (for ARM64/Apple Silicon)
                import subprocess
                result = subprocess.run(['which', 'chromium'], capture_output=True, text=True)
                if result.returncode == 0:
                    options.binary_location = '/usr/bin/chromium'
                    # Set chromium-driver path
                    from selenium.webdriver.chrome.service import Service
                    service = Service('/usr/bin/chromedriver')
                    driver = webdriver.Chrome(service=service, options=options)
                else:
                    # Fall back to Chrome
                    driver = webdriver.Chrome(options=options)
            except Exception as e:
                logging.warning(f"Failed to start Chrome/Chromium: {e}")
                # Try with explicit service path
                try:
                    from selenium.webdriver.chrome.service import Service
                    service = Service('/usr/local/bin/chromedriver')
                    driver = webdriver.Chrome(service=service, options=options)
                except Exception as e2:
                    logging.error(f"All Chrome driver attempts failed: {e2}")
                    return []
            
            if not driver:
                return []
            
            try:
                driver.get("https://www.theinformation.com/")
                
                # Wait for content to load
                WebDriverWait(driver, 10).until(
                    EC.presence_of_element_located((By.TAG_NAME, "a"))
                )
                
                # Find article links
                article_links = driver.find_elements(By.CSS_SELECTOR, "a[href*='/articles/']")
                seen_links = set()
                
                for link_element in article_links[:20]:
                    try:
                        href = link_element.get_attribute('href')
                        title = link_element.text.strip()
                        
                        if not href or not title or href in seen_links or len(title) < 10:
                            continue
                            
                        seen_links.add(href)
                        
                        # Try to extract full content (may be limited due to paywall)
                        full_content = extractor.extract_full_content(href, "The Information")
                        
                        article = {
                            "title": title,
                            "link": href,
                            "summary": title,
                            "published": "",
                            "guid": href,
                            "categories": [],
                            "source": "The Information",
                            "content": full_content or title
                        }
                        
                        articles.append(article)
                        
                        if len(articles) >= 20:
                            break
                            
                        # Rate limiting
                        time.sleep(random.uniform(2, 3))
                        
                    except Exception as e:
                        logging.warning(f"Error processing link: {e}")
                        continue
                        
            finally:
                driver.quit()
                
        except Exception as e:
            logging.error(f"Browser automation failed for The Information: {e}")
            
        logging.info(f"Successfully scraped {len(articles)} The Information articles")
        return articles

    def parse_article_content(article):
        """Parse article to extract only title and main content, removing metadata and formatting."""
        import re
        
        title = article.get('title', 'No Title')
        content = article.get('content', '')
        
        # Remove common unwanted elements
        # Remove attribution lines like "This article originally appeared on..."
        content = re.sub(r'This article originally appeared on.*?\.', '', content, flags=re.IGNORECASE)
        
        # Remove "About our ads" and similar footer content
        content = re.sub(r'About our ads.*$', '', content, flags=re.IGNORECASE | re.DOTALL)
        
        # Remove source attribution at the end
        content = re.sub(r'\s+[A-Z][a-z]+(?:\s+[A-Z][a-z]+)*\s*$', '', content)
        
        # Remove HTML-like remnants and weird characters
        content = re.sub(r'<[^>]+>', '', content)  # Remove any remaining HTML tags
        content = re.sub(r'\s+', ' ', content)     # Normalize whitespace
        content = content.strip()
        
        # Remove common newsletter/subscription calls to action
        unwanted_phrases = [
            "Subscribe to our newsletter",
            "Sign up for our newsletter", 
            "Follow us on",
            "If you buy something through a link in this article, we may earn commission",
            "Why you can trust us Engadget has been testing and reviewing consumer tech since 2004",
            "Our stories may include affiliate links; if you buy something through a link, we may earn a commission",
            "about how we evaluate products"
        ]
        
        for phrase in unwanted_phrases:
            content = re.sub(re.escape(phrase) + r'.*?\.', '', content, flags=re.IGNORECASE)
        
        return {
            'title': title,
            'content': content
        }



    @task
    def summarize_with_openrouter(all_articles: dict) -> str:
        """
        Generate a comprehensive summary of all tech articles using OpenRouter API.
        
        Args:
            all_articles: Dictionary with source names as keys and article lists as values
            
        Returns:
            str: Comprehensive summary of all articles
        """
        from airflow.models import Variable
        
        try:
            # Get OpenRouter API key from Airflow Variables
            openrouter_api_key = Variable.get("OPENROUTER_API_KEY")
        except Exception as e:
            logging.error(f"Failed to get OpenRouter API key: {e}")
            return "Error: OpenRouter API key not configured"
        
        # Parse all articles to get clean content
        all_parsed_articles = []
        total_articles = 0
        
        for source_name, articles in all_articles.items():
            for article in articles:
                parsed = parse_article_content(article)
                all_parsed_articles.append({
                    'source': source_name,
                    'title': parsed['title'],
                    'content': parsed['content']
                })
                total_articles += 1
        
        if not all_parsed_articles:
            return "No articles found to summarize."
        
        # Create comprehensive prompt with XML-style formatting
        prompt = f"""You are an expert tech journalist and analyst tasked with creating a comprehensive daily tech news summary. 

MISSION: Analyze {total_articles} tech articles from {len(all_articles)} major sources (TechCrunch, Engadget, Gizmodo, The Information) and create an executive-level briefing that captures the most important developments, trends, and insights.

CRITICAL OUTPUT INSTRUCTIONS:
- START IMMEDIATELY with the formatted briefing
- Do NOT include any analysis, thinking process, or explanatory text
- Do NOT show your reasoning or methodology
- Do NOT write "Let me analyze" or "I need to" or similar phrases
- Go DIRECTLY to the formatted output using the XML tags below
- Your first line should be a <title> or <category> tag

ANALYSIS FRAMEWORK (USE INTERNALLY - DO NOT SHOW):
1. MAJOR HEADLINES & BREAKING NEWS: Identify the 3-5 most significant stories that will impact the tech industry
2. EMERGING TRENDS & PATTERNS: Spot recurring themes, new technologies, or shifts in industry focus
3. COMPANY SPOTLIGHT: Highlight major corporate moves, product launches, acquisitions, or strategic changes
4. MARKET IMPLICATIONS: Assess potential impact on markets, consumers, and the broader tech ecosystem
5. INNOVATION HIGHLIGHTS: Call out genuinely innovative technologies or breakthrough announcements
6. REGULATORY & POLICY: Note any government actions, legal developments, or policy changes affecting tech

REQUIRED OUTPUT FORMAT:
Create a structured summary using these XML-style tags for formatting:

<category>TOP STORIES</category>
- List 3-5 most impactful stories with brief analysis of why they matter

<category>MARKET & BUSINESS MOVES</category>
- Major corporate developments, funding rounds, acquisitions, strategic partnerships

<category>TECHNOLOGY & INNOVATION</category>
- New product launches, breakthrough technologies, significant updates

<category>REGULATORY & LEGAL</category>
- Government actions, court decisions, policy changes affecting tech

<category>TREND ANALYSIS</category>
- Emerging patterns, industry shifts, what to watch

<category>KEY TAKEAWAYS</category>
- 3-4 bullet points summarizing the day's most important insights for tech executives

FORMATTING GUIDELINES:
- Use <title>Title Text</title> for major headings
- Use <bold>Important Text</bold> for emphasis instead of **bold**
- Use <category>Section Name</category> for section headers
- Do NOT use markdown formatting like ## or **
- Do NOT use table formatting with lines or pipes
- Use simple bullet points with - or •
- Keep formatting clean and XML-compatible

WRITING STYLE:
- Professional yet engaging tone suitable for C-level executives
- Focus on business implications and strategic insights
- Use precise, industry-appropriate terminology
- Be concise but comprehensive
- Include specific details like company names, product names, and key figures when relevant

QUALITY STANDARDS:
- Prioritize accuracy and factual reporting
- Avoid speculation unless clearly marked as such
- Focus on stories with broad industry relevance
- Synthesize information rather than just listing articles
- Provide actionable intelligence for business decision-makers

Here are the articles to analyze:

"""

        # Add all articles to prompt
        for i, article in enumerate(all_parsed_articles, 1):
            prompt += f"\n--- ARTICLE {i} ({article['source']}) ---\n"
            prompt += f"TITLE: {article['title']}\n"
            prompt += f"CONTENT: {article['content']}\n"
            prompt += f"{'='*80}\n"

        prompt += f"""

FINAL REMINDER: 
- Begin your response IMMEDIATELY with formatted XML output
- Do NOT write any explanatory text, analysis, or methodology
- Your very first words should be a <title> or <category> tag
- Provide ONLY the formatted briefing content as specified above
- Remember: Your goal is to create a valuable daily briefing that helps tech executives understand what happened today and why it matters for their business strategies."""

        # Define fallback models (free models from OpenRouter)
        fallback_models = [
            "openai/gpt-oss-20b:free",           # Original model
            "huggingfaceh4/zephyr-7b-beta:free", # HuggingFace Zephyr
            "mistralai/mistral-7b-instruct:free", # Mistral 7B
            "openchat/openchat-7b:free",          # OpenChat
            "meta-llama/llama-3.2-3b-instruct:free", # Llama 3.2
            "microsoft/wizardlm-2-8x22b:free",   # WizardLM
            "google/gemma-7b-it:free",           # Google Gemma
        ]
        
        headers = {
            "Authorization": f"Bearer {openrouter_api_key}",
            "Content-Type": "application/json",
            "HTTP-Referer": "https://tech-news-publisher.com",
            "X-Title": "Tech News Publisher DAG",
        }
        
        # Try each model with exponential backoff for rate limits
        for model_index, model in enumerate(fallback_models):
            payload = {
                "model": model,
                "messages": [
                    {
                        "role": "user", 
                        "content": prompt
                    }
                ]
            }
            
            # Try the current model with retries for rate limits
            for retry_attempt in range(3):  # Max 3 retries per model
                try:
                    if model_index == 0 and retry_attempt == 0:
                        logging.info(f"Generating comprehensive tech news summary with OpenRouter...")
                    else:
                        logging.info(f"Trying model {model} (attempt {retry_attempt + 1})")
                    
                    response = requests.post(
                        url="https://openrouter.ai/api/v1/chat/completions",
                        headers=headers,
                        data=json.dumps(payload),
                        timeout=60
                    )
                    response.raise_for_status()
                    
                    result = response.json()
                    summary = result['choices'][0]['message']['content']
                    
                    logging.info(f"✅ Successfully generated summary using model: {model}")
                    
                    # Serialize summary to JSON and write to host-mounted logs folder
                    try:
                        output_payload = {
                            "model": model,
                            "generated_at": time.strftime('%Y-%m-%dT%H:%M:%SZ', time.gmtime()),
                            "sources": list(all_articles.keys()),
                            "num_articles": total_articles,
                            "summary": summary,
                            "fallback_used": model_index > 0,
                            "retry_attempt": retry_attempt + 1
                        }
                        summaries_dir = "/opt/airflow/logs/summaries"
                        os.makedirs(summaries_dir, exist_ok=True)
                        file_name = f"summary_{int(time.time())}.json"
                        file_path = f"{summaries_dir}/{file_name}"
                        with open(file_path, "w", encoding="utf-8") as f:
                            json.dump(output_payload, f, ensure_ascii=False, indent=2)
                        logging.info(f"📄 Summary JSON written to {file_path}")
                        print(f"\nSummary saved to: {file_path}")
                        print(f"Model used: {model}")
                    except Exception as write_err:
                        logging.error(f"Failed to write summary JSON: {write_err}")
                    
                    return summary
                    
                except requests.exceptions.HTTPError as e:
                    if e.response.status_code == 429:  # Rate limit
                        wait_time = (2 ** retry_attempt) * 5  # Exponential backoff: 5s, 10s, 20s
                        logging.warning(f"Rate limited for {model}. Waiting {wait_time}s before retry...")
                        time.sleep(wait_time)
                        continue
                    else:
                        logging.warning(f"HTTP error for {model}: {e}")
                        break  # Try next model
                        
                except requests.exceptions.RequestException as e:
                    logging.warning(f"Request failed for {model}: {e}")
                    break  # Try next model
                    
                except Exception as e:
                    logging.warning(f"Unexpected error for {model}: {e}")
                    break  # Try next model
            
            # If we get here, all retries for this model failed
            logging.warning(f"All retries exhausted for model: {model}")
        
        # If all models failed
        error_msg = f"All {len(fallback_models)} OpenRouter models failed. Please check API key and try again later."
        logging.error(error_msg)
        
        # Still write an error JSON for debugging
        try:
            error_payload = {
                "error": error_msg,
                "generated_at": time.strftime('%Y-%m-%dT%H:%M:%SZ', time.gmtime()),
                "sources": list(all_articles.keys()),
                "num_articles": total_articles,
                "models_attempted": fallback_models
            }
            summaries_dir = "/opt/airflow/logs/summaries"
            os.makedirs(summaries_dir, exist_ok=True)
            file_name = f"error_{int(time.time())}.json"
            file_path = f"{summaries_dir}/{file_name}"
            with open(file_path, "w", encoding="utf-8") as f:
                json.dump(error_payload, f, ensure_ascii=False, indent=2)
            logging.info(f"📄 Error log written to {file_path}")
        except Exception as write_err:
            logging.error(f"Failed to write error JSON: {write_err}")
        
        return error_msg

    @task
    def safe_scrape_techcrunch() -> list[dict]:
        """Safe wrapper for TechCrunch scraper."""
        try:
            result = scrape_techcrunch.function()
            return result if result else []
        except Exception as e:
            logging.error(f"TechCrunch scraper failed: {e}")
            return []

    @task
    def safe_scrape_engadget() -> list[dict]:
        """Safe wrapper for Engadget scraper."""
        try:
            result = scrape_engadget.function()
            return result if result else []
        except Exception as e:
            logging.error(f"Engadget scraper failed: {e}")
            return []

    # @task
    # def safe_scrape_gizmodo() -> list[dict]:
    #     """Safe wrapper for Gizmodo scraper (disabled by request)."""
    #     try:
    #         result = scrape_gizmodo.function()
    #         return result if result else []
    #     except Exception as e:
    #         logging.error(f"Gizmodo scraper failed: {e}")
    #         return []

    @task
    def empty_gizmodo() -> list[dict]:
        """Disabled Gizmodo scraper replacement returning an empty list."""
        logging.info("Gizmodo scraper disabled - returning empty list")
        return []

    @task
    def safe_scrape_the_information() -> list[dict]:
        """Safe wrapper for The Information scraper."""
        try:
            result = scrape_the_information.function()
            return result if result else []
        except Exception as e:
            logging.error(f"The Information scraper failed: {e}")
            return []

    @task
    def save_successful_articles(tc_articles: list, eng_articles: list, giz_articles: list, inf_articles: list) -> dict:
        """Save only successful scraper results to JSON."""
        all_articles = {}
        
        # Safely handle inputs (convert None to empty list)
        tc_articles = tc_articles if tc_articles is not None else []
        eng_articles = eng_articles if eng_articles is not None else []
        giz_articles = giz_articles if giz_articles is not None else []
        inf_articles = inf_articles if inf_articles is not None else []
        
        # Only include sources with articles
        if tc_articles and len(tc_articles) > 0:
            all_articles["TechCrunch"] = tc_articles
            logging.info(f"✅ TechCrunch: {len(tc_articles)} articles")
        else:
            logging.warning("❌ TechCrunch: No articles")
        
        if eng_articles and len(eng_articles) > 0:
            all_articles["Engadget"] = eng_articles
            logging.info(f"✅ Engadget: {len(eng_articles)} articles")
        else:
            logging.warning("❌ Engadget: No articles")
        
        if giz_articles and len(giz_articles) > 0:
            all_articles["Gizmodo"] = giz_articles
            logging.info(f"✅ Gizmodo: {len(giz_articles)} articles")
        else:
            logging.warning("❌ Gizmodo: No articles")
        
        if inf_articles and len(inf_articles) > 0:
            all_articles["The Information"] = inf_articles
            logging.info(f"✅ The Information: {len(inf_articles)} articles")
        else:
            logging.warning("❌ The Information: No articles")
        
        # Save articles to JSON
        try:
            articles_dir = "/opt/airflow/logs/articles"
            os.makedirs(articles_dir, exist_ok=True)
            articles_file = f"{articles_dir}/articles_{int(time.time())}.json"
            
            # Include metadata about the scraping run
            save_data = {
                "timestamp": time.strftime('%Y-%m-%dT%H:%M:%SZ', time.gmtime()),
                "total_sources": len(all_articles),
                "successful_sources": list(all_articles.keys()),
                "articles": all_articles
            }
            
            with open(articles_file, "w", encoding="utf-8") as f:
                json.dump(save_data, f, ensure_ascii=False, indent=2)
            logging.info(f"📄 Articles saved to {articles_file} ({len(all_articles)} sources)")
        except Exception as e:
            logging.error(f"Failed to save articles JSON: {e}")
        
        return all_articles

    @task
    def generate_summary(all_articles: dict) -> str:
        """Generate summary with OpenRouter from successful articles."""
        if not all_articles:
            error_msg = "No articles available for summarization"
            logging.error(f"💥 {error_msg}")
            return error_msg
        
        total_articles = sum(len(articles) for articles in all_articles.values())
        logging.info(f"🚀 Generating summary for {total_articles} articles from {len(all_articles)} sources: {list(all_articles.keys())}")
        
        return summarize_with_openrouter.function(all_articles)

    @task
    def save_summary(summary: str) -> str:
        """Save the final summary to JSON."""
        try:
            summary_data = {
                "generated_at": time.strftime('%Y-%m-%dT%H:%M:%SZ', time.gmtime()),
                "summary": summary,
                "workflow_completed": True
            }
            
            summaries_dir = "/opt/airflow/logs/summaries"
            os.makedirs(summaries_dir, exist_ok=True)
            summary_file = f"{summaries_dir}/final_summary_{int(time.time())}.json"
            
            with open(summary_file, "w", encoding="utf-8") as f:
                json.dump(summary_data, f, ensure_ascii=False, indent=2)
            logging.info(f"📄 Final summary saved to {summary_file}")
            
            return f"Summary completed and saved to {summary_file}"
        except Exception as e:
            logging.error(f"Failed to save summary JSON: {e}")
            return f"Summary generated but save failed: {e}"

    @task
    def post_summary_to_backend(summary: str) -> str:
        """POST the final summary to BACKEND_AI/api/ai/tech-news with AIRFLOW_SECRET."""
        from airflow.models import Variable
        try:
            backend_url = Variable.get("BACKEND_AI")
            airflow_secret = Variable.get("AIRFLOW_SECRET")
        except Exception as e:
            logging.error(f"Missing Airflow Variables BACKEND_AI or AIRFLOW_SECRET: {e}")
            return f"Failed: Missing variables BACKEND_AI/AIRFLOW_SECRET"

        if not summary or summary.strip() == "":
            logging.error("Empty summary - skipping backend POST")
            return "Failed: Empty summary"

        # Build payload
        payload = {
            "summary": summary,
            "generated_at": time.strftime('%Y-%m-%dT%H:%M:%SZ', time.gmtime()),
        }

        # Build headers with secret
        headers = {
            "Content-Type": "application/json",
            "X-Airflow-Secret": airflow_secret,
        }

        # Compose full URL
        url = backend_url.rstrip('/') + "/api/ai/tech-news"

        try:
            resp = requests.post(url, headers=headers, data=json.dumps(payload), timeout=30)
            if resp.status_code >= 200 and resp.status_code < 300:
                logging.info(f"✅ Posted summary to backend: {url}")
                return f"Posted to backend: {url}"
            else:
                logging.error(f"Backend POST failed ({resp.status_code}): {resp.text}")
                return f"Failed: {resp.status_code}"
        except Exception as e:
            logging.error(f"Backend POST exception: {e}")
            return f"Failed: {e}"

    # Execute workflow with proper task dependencies
    tc_articles = safe_scrape_techcrunch()
    eng_articles = safe_scrape_engadget()
    # giz_articles = safe_scrape_gizmodo()  # disabled
    giz_articles = empty_gizmodo()
    inf_articles = safe_scrape_the_information()
    
    # Save successful articles
    saved_articles = save_successful_articles(tc_articles, eng_articles, giz_articles, inf_articles)
    
    # Generate summary from saved articles
    summary = generate_summary(saved_articles)
    
    # Save final summary locally
    final_result = save_summary(summary)
    
    # Post to backend API with secret
    post_status = post_summary_to_backend(summary)

tech_news_publisher_dag()
 