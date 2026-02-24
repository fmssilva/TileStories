/**
 * STATICFORMS API TEST SCRIPT
 * ============================
 * 
 * This script tests the StaticForms API to understand exact response formats
 * for different scenarios (success, errors, invalid data, etc.)
 * 
 * Run with: node test-staticforms-api.js
 */

const API_URL = 'https://api.staticforms.xyz/submit';
const ACCESS_KEY = 'sf_6ki977i47881kfjjig1i32f4';

async function testAPICall(testName, payload) {
    console.log(`\n${'='.repeat(60)}`);
    console.log(`TEST: ${testName}`);
    console.log(`${'='.repeat(60)}`);
    console.log('Request payload:', JSON.stringify(payload, null, 2));

    try {
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        console.log('\n--- RESPONSE ---');
        console.log('Status Code:', response.status);
        console.log('Status Text:', response.statusText);
        console.log('OK:', response.ok);

        const responseText = await response.text();
        console.log('Raw Response:', responseText);

        try {
            const jsonData = JSON.parse(responseText);
            console.log('Parsed JSON:', JSON.stringify(jsonData, null, 2));
        } catch (e) {
            console.log('Response is not JSON');
        }

    } catch (error) {
        console.log('\n--- ERROR ---');
        console.log('Error Type:', error.constructor.name);
        console.log('Error Message:', error.message);
    }
}

async function runAllTests() {
    console.log('STATICFORMS API TESTING');
    console.log('Starting tests...\n');

    // Test 1: Valid submission (should succeed)
    await testAPICall('Valid Submission', {
        accessKey: ACCESS_KEY,
        name: 'Test User',
        email: 'test@example.com',
        subject: 'Test Subject',
        message: 'This is a test message',
        replyTo: 'test@example.com'
    });

    // Test 2: Missing required field
    await testAPICall('Missing Email Field', {
        accessKey: ACCESS_KEY,
        name: 'Test User',
        subject: 'Test Subject',
        message: 'This is a test message'
    });

    // Test 3: Invalid email format
    await testAPICall('Invalid Email Format', {
        accessKey: ACCESS_KEY,
        name: 'Test User',
        email: 'not-an-email',
        subject: 'Test Subject',
        message: 'This is a test message',
        replyTo: 'not-an-email'
    });

    // Test 4: Invalid access key
    await testAPICall('Invalid Access Key', {
        accessKey: 'invalid_key_12345',
        name: 'Test User',
        email: 'test@example.com',
        subject: 'Test Subject',
        message: 'This is a test message',
        replyTo: 'test@example.com'
    });

    // Test 5: Empty fields
    await testAPICall('Empty Fields', {
        accessKey: ACCESS_KEY,
        name: '',
        email: '',
        subject: '',
        message: ''
    });

    console.log('\n' + '='.repeat(60));
    console.log('ALL TESTS COMPLETED');
    console.log('='.repeat(60));
}

// Run all tests
runAllTests().catch(error => {
    console.error('Fatal error:', error);
});
