import os
from datetime import datetime
import urllib3

SITE = os.environ['site']  # URL of the site to check, stored in the site environment variable
CACHE_VALIDATION_KEY = os.environ['CacheValidationKey']  

def update_stores(http):
    response = http.request(
        'GET',
        SITE + "stores",
        headers={
            'User-Agent': 'AWS Lambda',
            "AWS-Key": CACHE_VALIDATION_KEY
        }
    )
    print(response.data)


def lambda_handler(event, context):
    print('Checking at {}...'.format(str(datetime.now())))
    try:
        http = urllib3.PoolManager()
        update_stores(http)
    except:
        print('Check failed!')
        raise
    else:
        print('Check passed!')
    finally:
        print('Check complete at {}'.format(str(datetime.now())))
