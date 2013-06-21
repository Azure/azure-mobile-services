Domenic Denicola has written a test suite for Promises/A+ compliance: https://github.com/promises-aplus/promises-tests/

You can run that test suite against our Mobile Services Web SDK by running the following commands in this directory:

    npm install
    node test-promises.js

Why a separate testing system?
============================================
Although we could convert this test suite into the $testGroup/$test/$assert format needed for compatibility with
our other client tests, it's not completely trivial because these Node tests rely on a Node-specific mocking library
and in any case we would then lose the ability to upgrade easily to newer versions of the test suite.