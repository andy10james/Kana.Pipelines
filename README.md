Functional pipeline classes for building result driven or operation driven, modular pipelines. 

## Why?

The pipeline pattern allows you to build reusable, abstract, and self-contained middleware functions that can be integrated into different pipelines to achieve any number of larger tasks without duplicating code. For pipeline builders the middleware could be defined anywhere, such as given in via user parameters, self-defined or from libraries. This pattern is most most recongisable from ASP.NET Core, where middleware from many sources (UseMvc, UseStaticFiles, UseDeveloperExceptionPage etc.) integrate together to handle web request between them. 

## Example Snippet (with inline functions)

Pipeline definition:

```csharp
var authenticationPipeline = new Pipeline<SignInInput, string>();

authenticationPipeline.Add(UserIsDeleted);
authenticationPipeline.Add(UserIsTemporary);
authenticationPipeline.Add(CredentialsCorrect);

var input = new User("kana_ki_", "randomPassword");
var output = await authenticationPipeline.RunAsync(input);
```

Middleware definitions:

```csharp
public async Task<string> UserIsDeleted(SignInInput signIn, Func<Task<string>> next) {
  if (this.db.FindByUsername(signIn.Username).IsDeleted) {
    return "User deleted";
  }
  return await next();
}

public async Task<string> UserIsTemporary(SignInInput signIn, Func<Task<string>> next) {
  var result = await next();
  if (result == "User authenticated" && this.db.FindByUsername(signIn.Username).IsTemporary) {
    result = "User authenticated temporarily";
  }
  return result;
}

public async Task<string> CredentialsCorrect(SignInInput signIn, Func<Task<string>> next) {
  if (this.db.FindByUsername(signIn.Username).Password == hash(signIn.Password)) {
    return "User authenticated";
  }
  return "User not authenticated";
}
```

In the above example, 3 middleware functions are declared and are queued in the pipeline in the order `UserIsDeleted`, `UserIsTemporary` and `CredentialsCorrect`. Each middleware in turn has the opportunity to return a result and stop the pipeline from continuing, if they do the result propogates through the previous middlewares in the pipeline (later middlewares wouldn't run). 

The `UserIsDeleted` middleware immediately concludes the pipeline if the user is deleted, otherwise it calls `next()` to pass on to the next middleware in the pipeline. 

`UserIsTemporary` immediately passes off to the next middleware in the pipeline - the `CredentialsCorrect` middleware that checks the sign in detail. Once a result is received from the remainder of the pipeline it checks to see if it was a "User authenticated" result, if so and the user is a temporary user, then it modifies the pipeline result to say "User authenticated temporarily". 

Each middleware knows nothing about the other middlewares in the pipeline, and such should work even if other middleware is swapped out for an alternate middleware. Building out tasks in a pipeline pattern like this can really help modularise responsibility and functionality, particularly when those tasks are complicated and/or could be achieved in a variety of combinations of operations. 

## Example Snippet (with injected types)

Pipeline definition:

```csharp
var authenticationPipeline =
  new Pipeline<SignInInput, string>()
    .WithServiceProvider(serviceProvider)
    .Add<UserIsDeletedMiddleware>();
    .Add<UserIsTemporaryMiddleware>();
    .Add<CredentialsCorrectMiddleware>();

var input = new User("kana_ki_", "randomPassword");
var output = await authenticationPipeline.RunAsync(input);
```

Middleware definitions:

```csharp
public class UserIsDeletedMiddleware : IMiddleware<SignInInput, string> {

  private IDatabase db;

  public UserIdDeletedMiddleware(IDatabase db) {
    this.db = repo;
  }

  public async Task<string> ExecuteAsync(SignInInput signIn, Func<Task<string>> next) {
    if (this.db.FindByUsername(signIn.Username).IsDeleted) {
      return "User deleted";
    }
    return await next();
  }
  
}

public class UserIsTemporaryMiddleware : IMiddleware<SignInInput, string> {

  private IDatabase db;

  public UserIsTemporaryMiddleware(IDatabase db) {
    this.db = repo;
  }

  public async Task<string> ExecuteAsync(SignInInput signIn, Func<Task<string>> next) {
    var result = await next();
    if (result == "User authenticated" && this.db.FindByUsername(signIn.Username).IsTemporary) {
      result = "User authenticated temporarily";
    }
    return result;
  }
  
}

public class CredentialsCorrectMiddleware : IMiddleware<SignInInput, string> {

  private IDatabase db;

  public CredentialsCorrectMiddleware(IDatabase db) {
    this.db = repo;
  }

  public async Task<string> ExecuteAsync(SignInInput signIn, Func<Task<string>> next) {
    if (this.db.FindByUsername(signIn.Username).Password == hash(signIn.Password)) {
      return "User authenticated";
    }
    return "User not authenticated";
  }
  
}
```

## Features

 - Pipelines that do not conclude with a result (void pipelines). 
 - Pipelines that do conclude with a result. 
 - Middleware can conclude the pipeline early with a result, or modify the result from later middleware. 
 - Middleware can be provided as inline functions, instances of IPipeline, or as references to types of IPipeline that are JIT constructor and dependency injected if given a service provider. 
 - Pipelines can be made from other pipelines, making new union pipelines, via flexible Add overloads and the native C# `+` operator.

